using CSEVirtualLabDataAccessLayer;
using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;

namespace CSEVirtualLabWebAPI.Services
{
    public class LabReportService
    {
        private readonly VirtualLabRepository repository;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public LabReportService(
            VirtualLabRepository repository,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            this.repository = repository;
            this.environment = environment;
            this.configuration = configuration;
        }

        public async Task<GeneratedReportFile?> GenerateLabReportAsync(
            int userId,
            int labId)
        {
            LabReportDto? report =
                await repository.GetLabReportDataAsync(
                    userId,
                    labId);

            if (report == null)
            {
                return null;
            }

            string templatePath =
                GetReportTemplatePath(report);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    "Report template was not found.",
                    templatePath);
            }

            string outputDirectory =
                Path.Combine(
                    environment.ContentRootPath,
                    "wwwroot",
                    "generated-reports");

            Directory.CreateDirectory(outputDirectory);

            string fileName =
                $"{GetSafeFileName(report.StudentName)}-{GetSafeFileName(report.Usn)}.docx";

            string outputPath =
                Path.Combine(
                    outputDirectory,
                    fileName);

            File.Copy(
                templatePath,
                outputPath,
                true);

            FillReportTemplate(
                outputPath,
                report);

            string pdfPath =
                ConvertDocxToPdf(
                    outputPath,
                    outputDirectory);

            string pdfFileName =
                Path.ChangeExtension(
                    fileName,
                    ".pdf");

            return new GeneratedReportFile
            {
                FileName = pdfFileName,
                ContentType = "application/pdf",
                Content = await File.ReadAllBytesAsync(pdfPath)
            };
        }

        private string GetReportTemplatePath(
            LabReportDto report)
        {
            string angularProjectRoot =
                Path.GetFullPath(
                    Path.Combine(
                        environment.ContentRootPath,
                        "..",
                        "..",
                        "CSEVirtualLab"));

            return Path.Combine(
                angularProjectRoot,
                "src",
                "assets",
                "Reports",
                "C_Lab_Report_Template.docx");
        }

        private string ConvertDocxToPdf(
            string docxPath,
            string outputDirectory)
        {
            string libreOfficePath =
                GetLibreOfficePath();

            string pdfPath =
                Path.ChangeExtension(
                    docxPath,
                    ".pdf");

            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }

            var startInfo =
                new ProcessStartInfo
                {
                    FileName = libreOfficePath,
                    Arguments =
                        $"--headless --convert-to pdf --outdir \"{outputDirectory}\" \"{docxPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

            using Process process =
                new Process
                {
                    StartInfo = startInfo
                };

            process.Start();

            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync();

            Task<string> errorTask =
                process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(60000))
            {
                process.Kill(true);

                throw new TimeoutException(
                    "PDF conversion timed out.");
            }

            string output =
                outputTask.Result;

            string error =
                errorTask.Result;

            if (process.ExitCode != 0 || !File.Exists(pdfPath))
            {
                throw new InvalidOperationException(
                    $"PDF conversion failed. Output: {output} Error: {error}");
            }

            return pdfPath;
        }

        private string GetLibreOfficePath()
        {
            string? configuredPath =
                configuration["ReportGeneration:LibreOfficePath"];

            if (!string.IsNullOrWhiteSpace(configuredPath) &&
                File.Exists(configuredPath))
            {
                return configuredPath;
            }

            string[] possiblePaths =
            {
                @"C:\Program Files\LibreOffice\program\soffice.exe",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
            };

            string? libreOfficePath =
                possiblePaths.FirstOrDefault(File.Exists);

            if (libreOfficePath == null)
            {
                throw new FileNotFoundException(
                    "LibreOffice soffice.exe was not found. Install LibreOffice or configure ReportGeneration:LibreOfficePath in appsettings.json.");
            }

            return libreOfficePath;
        }

        private static void FillReportTemplate(
            string docxPath,
            LabReportDto report)
        {
            XNamespace w =
                "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            using ZipArchive archive =
                ZipFile.Open(
                    docxPath,
                    ZipArchiveMode.Update);

            ZipArchiveEntry documentEntry =
                archive.GetEntry("word/document.xml")
                ?? throw new InvalidOperationException(
                    "The Word document body was not found.");

            XDocument document;

            using (Stream stream = documentEntry.Open())
            {
                document =
                    XDocument.Load(
                        stream,
                        LoadOptions.PreserveWhitespace);
            }

            List<XElement> tables =
                document
                    .Descendants(w + "tbl")
                    .ToList();

            if (tables.Count < 2)
            {
                throw new InvalidOperationException(
                    "The report template must contain two tables.");
            }

            FillStudentDetailsTable(
                tables[0],
                report,
                w);

            FillExperimentProgressTable(
                tables[1],
                report,
                w);

            documentEntry.Delete();

            ZipArchiveEntry newDocumentEntry =
                archive.CreateEntry("word/document.xml");

            using Stream outputStream =
                newDocumentEntry.Open();

            document.Save(
                outputStream,
                SaveOptions.DisableFormatting);
        }

        private static void FillStudentDetailsTable(
            XElement table,
            LabReportDto report,
            XNamespace w)
        {
            List<XElement> rows =
                table
                    .Elements(w + "tr")
                    .ToList();

            SetValueCell(rows, 0, report.StudentName, w);
            SetValueCell(rows, 1, report.Usn, w);
            SetValueCell(rows, 2, report.Department, w);
            SetValueCell(rows, 3, report.College, w);
            SetValueCell(rows, 4, report.LabName, w);
            SetValueCell(
                rows,
                5,
                report.CanDownloadCertificate
                    ? "Completed"
                    : "Not Completed",
                w);
            SetValueCell(
                rows,
                6,
                FormatDate(report.DateOfRegistration),
                w);
            SetValueCell(
                rows,
                7,
                FormatDate(report.DateOfCompletion),
                w);
        }

        private static void FillExperimentProgressTable(
            XElement table,
            LabReportDto report,
            XNamespace w)
        {
            List<XElement> rows =
                table
                    .Elements(w + "tr")
                    .ToList();

            for (int index = 1; index <= 14 && index < rows.Count - 1; index++)
            {
                int experimentNumber =
                    index <= 8
                        ? index
                        : index;

                LabReportExperimentDto? experiment =
                    report.Experiments.FirstOrDefault(item =>
                        item.ExperimentNumber == experimentNumber);

                if (experiment == null)
                {
                    continue;
                }

                List<XElement> cells =
                    rows[index]
                        .Elements(w + "tc")
                        .ToList();

                if (cells.Count < 6)
                {
                    continue;
                }

                SetCellText(
                    cells[1],
                    experiment.PartExperimentNumber.ToString(),
                    w);
                SetCellText(
                    cells[2],
                    experiment.ExperimentTitle,
                    w);
                SetCellText(
                    cells[3],
                    experiment.Execution,
                    w);
                SetCellText(
                    cells[4],
                    experiment.Quiz,
                    w);
                SetCellText(
                    cells[5],
                    experiment.Assignments,
                    w);
            }

            XElement averageRow =
                rows.Last();

            List<XElement> averageCells =
                averageRow
                    .Elements(w + "tc")
                    .ToList();

            if (averageCells.Count >= 2)
            {
                SetCellText(
                    averageCells[1],
                    report.AverageQuizScore == 0
                        ? "-"
                        : report.AverageQuizScore.ToString("0.##"),
                    w);
            }
        }

        private static void SetValueCell(
            List<XElement> rows,
            int rowIndex,
            string value,
            XNamespace w)
        {
            if (rowIndex >= rows.Count)
            {
                return;
            }

            List<XElement> cells =
                rows[rowIndex]
                    .Elements(w + "tc")
                    .ToList();

            if (cells.Count < 2)
            {
                return;
            }

            SetCellText(
                cells[1],
                value,
                w);
        }

        private static void SetCellText(
            XElement cell,
            string value,
            XNamespace w)
        {
            XNamespace xml =
                XNamespace.Xml;

            cell
                .Elements(w + "p")
                .Remove();

            var paragraph =
                new XElement(
                    w + "p",
                    new XElement(
                        w + "pPr",
                        new XElement(
                            w + "jc",
                            new XAttribute(w + "val", "left"))),
                    new XElement(
                        w + "r",
                        new XElement(
                            w + "rPr",
                            new XElement(
                                w + "rFonts",
                                new XAttribute(w + "ascii", "Times New Roman"),
                                new XAttribute(w + "hAnsi", "Times New Roman"),
                                new XAttribute(w + "cs", "Times New Roman")),
                            new XElement(
                                w + "sz",
                                new XAttribute(w + "val", "24"))),
                        new XElement(
                            w + "t",
                            new XAttribute(xml + "space", "preserve"),
                            value ?? string.Empty)));

            cell.Add(paragraph);
        }

        private static string FormatDate(
            DateTime? date)
        {
            return date.HasValue
                ? date.Value.ToLocalTime().ToString("dd-MM-yyyy")
                : string.Empty;
        }

        private static string GetSafeFileName(
            string value)
        {
            char[] invalidCharacters =
                Path.GetInvalidFileNameChars();

            string cleaned =
                new string(
                    value
                        .Where(character =>
                            !invalidCharacters.Contains(character))
                        .ToArray());

            return cleaned
                .Trim()
                .Replace(" ", "_");
        }
    }

    public class GeneratedReportFile
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}
