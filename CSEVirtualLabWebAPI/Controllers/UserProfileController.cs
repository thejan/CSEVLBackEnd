using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabDataAccessLayer.Models;
using CSEVirtualLabWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/user-profile")]
    public class UserProfileController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public UserProfileController(
            VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateUserProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Registration? updatedUser =
                await repository.UpdateUserProfileAsync(
                    request.UserId,
                    request.StudentName,
                    request.College,
                    request.Semester);

            if (updatedUser == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User account was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Profile updated successfully.",
                user = new
                {
                    updatedUser.UserId,
                    updatedUser.StudentName,
                    updatedUser.Usn,
                    updatedUser.College,
                    updatedUser.Department,
                    updatedUser.Semester,
                    updatedUser.EmailId
                }
            });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool passwordChanged =
                    await repository.ChangePasswordAsync(
                        request.UserId,
                        request.CurrentPassword,
                        request.NewPassword);

                if (!passwordChanged)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User account was not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Password changed successfully."
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
    }
}
