using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabWebAPI.Models;
using CSEVirtualLabWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly VirtualLabRepository repository;
        private readonly RegistrationEmailService emailService;

        public AdminController(
            VirtualLabRepository repository,
            RegistrationEmailService emailService)
        {
            this.repository = repository;
            this.emailService = emailService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            AdminDashboardDto dashboard =
                await repository.GetAdminDashboardAsync();

            return Ok(dashboard);
        }

        [HttpGet("college-wise")]
        public async Task<IActionResult> GetCollegeWise()
        {
            return Ok(
                await repository.GetCollegeWiseRegistrationsAsync());
        }

        [HttpGet("department-wise")]
        public async Task<IActionResult> GetDepartmentWise()
        {
            return Ok(
                await repository.GetDepartmentWiseRegistrationsAsync());
        }

        [HttpGet("atmece-departments")]
        public async Task<IActionResult> GetAtmeceDepartments()
        {
            return Ok(
                await repository.GetAtmeceDepartmentWiseRegistrationsAsync());
        }

        [HttpGet("completion-status")]
        public async Task<IActionResult> GetCompletionStatus()
        {
            return Ok(
                await repository.GetLabCompletionStatusAsync());
        }

        [HttpGet("user-registrations")]
        public async Task<IActionResult> GetUserRegistrations()
        {
            return Ok(
                await repository.GetUserRegistrationsForAdminAsync());
        }

        [HttpGet("auto-approval")]
        public async Task<IActionResult> GetAutoApproval()
        {
            return Ok(new
            {
                enabled =
                    await repository
                        .GetAutoApproveRegistrationsAsync()
            });
        }

        [HttpPut("auto-approval")]
        public async Task<IActionResult> UpdateAutoApproval(
            [FromBody] UpdateAutoApprovalRequest request)
        {
            await repository.SetAutoApproveRegistrationsAsync(
                request.Enabled);

            return Ok(new
            {
                success = true,
                enabled = request.Enabled,
                message = request.Enabled
                    ? "Automatic registration approval is enabled."
                    : "Automatic registration approval is disabled."
            });
        }

        [HttpGet("user-requests")]
        public async Task<IActionResult> GetUserRequests()
        {
            return Ok(
                await repository.GetUserRequestsForAdminAsync());
        }

        [HttpPut("user-requests/{requestId:long}")]
        public async Task<IActionResult> UpdateUserRequest(
            long requestId,
            [FromBody] UpdateUserRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool updated =
                    await repository.UpdateUserRequestAsync(
                        requestId,
                        request.Status,
                        request.Remarks);

                if (!updated)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "The user request was not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User request updated successfully."
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }

        [HttpPut("registration-status")]
        public async Task<IActionResult> UpdateRegistrationStatus(
            [FromBody] UpdateUserRegistrationStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool updated =
                    await repository.UpdateUserRegistrationStatusAsync(
                        request.UserId,
                        request.RegistrationStatus,
                        request.ApprovedBy);

                if (!updated)
                {
                    return NotFound("User registration was not found.");
                }

                bool emailSent = false;

                if (
                    request.RegistrationStatus.Equals(
                        "Approved",
                        StringComparison.OrdinalIgnoreCase))
                {
                    string? email =
                        await repository.GetUserEmailAsync(
                            request.UserId);

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        emailSent =
                            await emailService
                                .SendApprovalEmailAsync(email);
                    }
                }

                return Ok(new
                {
                    success = true,
                    emailSent,
                    message =
                        request.RegistrationStatus.Equals(
                            "Approved",
                            StringComparison.OrdinalIgnoreCase)
                            ? emailSent
                                ? "User registration approved successfully and the approval email was sent."
                                : "User registration approved successfully, but the approval email could not be sent."
                            : "User registration rejected successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPut("registration-status/bulk")]
        public async Task<IActionResult> UpdateSelectedRegistrationStatus(
            [FromBody] BulkUserRegistrationStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int updatedCount =
                    await repository.UpdateSelectedUserRegistrationStatusAsync(
                        request.UserIds,
                        request.RegistrationStatus,
                        request.ApprovedBy);

                int emailsSent = 0;

                if (
                    request.RegistrationStatus.Equals(
                        "Approved",
                        StringComparison.OrdinalIgnoreCase))
                {
                    List<string> emails =
                        await repository.GetUserEmailsAsync(
                            request.UserIds);

                    foreach (string email in emails)
                    {
                        if (
                            await emailService
                                .SendApprovalEmailAsync(email))
                        {
                            emailsSent++;
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    updatedCount,
                    emailsSent,
                    message =
                        request.RegistrationStatus.Equals(
                            "Approved",
                            StringComparison.OrdinalIgnoreCase)
                            ? $"{updatedCount} user registration(s) approved. {emailsSent} approval email(s) sent."
                            : $"{updatedCount} user registration(s) rejected successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpDelete("user-registrations/{userId:int}")]
        public async Task<IActionResult> DeleteUserRegistration(
            int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "A valid UserId is required."
                });
            }

            try
            {
                bool deleted =
                    await repository
                        .DeleteUserRegistrationAsync(userId);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "The user registration was not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        $"User registration {userId} was deleted successfully."
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }
    }
}
