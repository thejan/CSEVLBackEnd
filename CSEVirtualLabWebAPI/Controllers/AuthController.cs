using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabDataAccessLayer.Models;
using CSEVirtualLabWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public AuthController(VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var registration =
                new Registration
                {
                    UserType = request.UserType,
                    College = request.College,
                    Organization = request.Organization,
                    Department = request.Department,
                    StudentName = request.StudentName,
                    Usn = request.Usn,
                    Semester = request.Semester,
                    Designation = request.Designation,
                    EmailId = request.EmailId,
                    PasswordHash = request.Password
                };

            try
            {
                Registration registeredUser =
                    await repository.RegisterUserAsync(registration);

                return Ok(new
                {
                    success = true,
                    message =
                        registeredUser.RegistrationStatus == "Approved"
                            ? "Registration completed and approved successfully. You can now log in."
                            : "Registration submitted successfully. Please wait for admin approval.",
                    registrationStatus =
                        registeredUser.RegistrationStatus,
                    userId = registeredUser.UserId
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Registration? user =
                await repository.LoginAsync(request.EmailId);

            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized(new
                {
                    success = false,
                    message =
                        "Invalid login details or registration is not approved."
                });
            }

            long sessionId =
                await repository.StartUserSessionAsync(user.UserId);

            return Ok(new
            {
                success = true,
                message = "Login successful.",
                sessionId,
                user = new
                {
                    user.UserId,
                    user.StudentName,
                    Usn = user.Usn ?? string.Empty,
                    College =
                        user.College ??
                        user.Organization ??
                        string.Empty,
                    user.Department,
                    Semester = user.Semester ?? 0,
                    user.EmailId,
                    user.UserType,
                    role =
                        user.Role?.RoleName ??
                        user.UserType
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool loggedOut =
                await repository.LogoutAsync(request.UserId);

            if (!loggedOut)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Open user session was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Logout recorded successfully."
            });
        }
    }
}
