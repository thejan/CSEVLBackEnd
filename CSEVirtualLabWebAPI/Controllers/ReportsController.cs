using CSEVirtualLabWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly LabReportService reportService;

        public ReportsController(
            LabReportService reportService)
        {
            this.reportService = reportService;
        }

        [HttpGet("lab-report")]
        public async Task<IActionResult> DownloadLabReport(
            [FromQuery] int userId,
            [FromQuery] int labId)
        {
            GeneratedReportFile? report =
                await reportService.GenerateLabReportAsync(
                    userId,
                    labId);

            if (report == null)
            {
                return NotFound(
                    "Report data was not found for the selected user and lab.");
            }

            return File(
                report.Content,
                report.ContentType,
                report.FileName);
        }
    }
}
