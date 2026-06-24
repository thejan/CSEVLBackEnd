using CSEVirtualLabWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/admin-auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly AdminSessionService sessionService;

        public AdminAuthController(
            IConfiguration configuration,
            AdminSessionService sessionService)
        {
            this.configuration = configuration;
            this.sessionService = sessionService;
        }

        [HttpPost("login")]
        public IActionResult Login(
            [FromBody] AdminLoginRequest request)
        {
            string configuredUsername =
                configuration["AdminLogin:Username"] ??
                string.Empty;

            string configuredPassword =
                configuration["AdminLogin:Password"] ??
                string.Empty;

            bool isValid =
                string.Equals(
                    request.Username?.Trim(),
                    configuredUsername,
                    StringComparison.Ordinal) &&
                string.Equals(
                    request.Password,
                    configuredPassword,
                    StringComparison.Ordinal);

            if (!isValid)
            {
                return Unauthorized(new
                {
                    success = false,
                    message =
                        "Invalid administrator username or password."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Administrator login successful.",
                token = sessionService.CreateSession()
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Request.Headers.TryGetValue(
                "X-Admin-Token",
                out var token);

            sessionService.EndSession(
                token.FirstOrDefault());

            return Ok(new
            {
                success = true,
                message = "Administrator logged out successfully."
            });
        }
    }

    public class AdminLoginRequest
    {
        public string Username { get; set; } =
            string.Empty;

        public string Password { get; set; } =
            string.Empty;
    }
}
