using System.Diagnostics;
using CSEVirtualLabWebAPI.Models;

namespace CSEVirtualLabWebAPI.Services
{
    public class CCompilerService
    {
        public async Task<CodeExecutionResponse> ExecuteCode(
            string sourceCode,
            string input)
        {
            string tempFolder =
                Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString());

            Directory.CreateDirectory(tempFolder);

            string sourceFile =
                Path.Combine(tempFolder, "program.c");

            string exeFile =
                Path.Combine(tempFolder, "program.exe");

            await File.WriteAllTextAsync(
                sourceFile,
                sourceCode);

            try
            {
                // =========================
                // COMPILE
                // =========================

                ProcessStartInfo compileInfo =
                    new ProcessStartInfo
                    {
                        FileName = @"C:\mingw64\bin\gcc.exe",

                        Arguments =
                            $"\"{sourceFile}\" -o \"{exeFile}\"",

                        RedirectStandardOutput = true,
                        RedirectStandardError = true,

                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                using var compileProcess =
                    Process.Start(compileInfo);

                string compileErrors =
                    await compileProcess.StandardError
                        .ReadToEndAsync();

                await compileProcess.WaitForExitAsync();

                if (compileProcess.ExitCode != 0)
                {
                    return new CodeExecutionResponse
                    {
                        Success = false,
                        Error = compileErrors
                    };
                }

                // =========================
                // EXECUTE
                // =========================

                ProcessStartInfo runInfo =
                    new ProcessStartInfo
                    {
                        FileName = exeFile,

                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,

                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                using var runProcess =
                    Process.Start(runInfo);

                // Send user input to scanf()

                if (!string.IsNullOrWhiteSpace(input))
                {
                    await runProcess.StandardInput
                        .WriteLineAsync(input);

                    runProcess.StandardInput.Close();
                }

                string output =
                    await runProcess.StandardOutput
                        .ReadToEndAsync();

                string runtimeError =
                    await runProcess.StandardError
                        .ReadToEndAsync();

                await runProcess.WaitForExitAsync();

                return new CodeExecutionResponse
                {
                    Success = string.IsNullOrEmpty(runtimeError),

                    Output = output,

                    Error = runtimeError
                };
            }
            catch (Exception ex)
            {
                return new CodeExecutionResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
            finally
            {
                try
                {
                    await Task.Delay(300);

                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(
                            tempFolder,
                            true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}