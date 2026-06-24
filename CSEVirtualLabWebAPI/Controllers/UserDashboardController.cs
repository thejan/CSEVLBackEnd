using CSEVirtualLabDataAccessLayer;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/user-dashboard")]
    public class UserDashboardController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public UserDashboardController(VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetDashboard(int userId)
        {
            UserDashboardDto? dashboard =
                await repository.GetUserDashboardAsync(userId);

            if (dashboard == null)
            {
                return NotFound("User dashboard data was not found.");
            }

            return Ok(dashboard);
        }

        [HttpGet("{userId:int}/log-history")]
        public async Task<IActionResult> GetLogHistory(int userId)
        {
            UserLogHistoryDto history =
                await repository.GetUserLogHistoryAsync(userId);

            return Ok(history);
        }

        [HttpGet("{userId:int}/certificate")]
        public async Task<IActionResult> GetCompletionCertificateData(
            int userId,
            [FromQuery] int labId)
        {
            CompletionCertificateDto? certificate =
                await repository.GetCompletionCertificateDataAsync(
                    userId,
                    labId);

            if (certificate == null)
            {
                return NotFound(
                    "Completion certificate data was not found.");
            }

            return Ok(certificate);
        }

        [HttpGet("{userId:int}/can-download-certificate")]
        public async Task<IActionResult> CanDownloadCertificate(
            int userId,
            [FromQuery] int labId)
        {
            bool canDownload =
                await repository.CanDownloadCertificateAsync(userId, labId);

            return Ok(new
            {
                userId,
                labId,
                canDownload
            });
        }
    }
}
