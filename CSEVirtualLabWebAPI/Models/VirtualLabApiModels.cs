using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSEVirtualLabWebAPI.Models
{
    public class RegisterUserRequest
    {
        [Required]
        public string UserType { get; set; } = "Student";

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string StudentName { get; set; } = string.Empty;

        public string? College { get; set; }

        public string? Organization { get; set; }

        public string? Usn { get; set; }

        [Range(1, 8)]
        public byte? Semester { get; set; }

        public string? Designation { get; set; }

        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        [Required]
        public int UserId { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        public string College { get; set; } = string.Empty;

        [Range(1, 8)]
        public byte Semester { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RegisterLabRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LabId { get; set; }
    }

    public class UpdateExecutionStatusRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LabId { get; set; }

        [Required]
        public int ExperimentId { get; set; }
    }

    public class UpdateQuizStatusRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LabId { get; set; }

        [Required]
        public int ExperimentId { get; set; }

        [Range(0, 100)]
        public int QuizScore { get; set; }

        [Range(1, 100)]
        public int QuizMaxMarks { get; set; }
    }

    public class UpdateAssignmentStatusRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LabId { get; set; }

        [Required]
        public int ExperimentId { get; set; }

        [Range(1, 5)]
        public int AssignmentId { get; set; }
    }

    public class UpdateUserRegistrationStatusRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RegistrationStatus { get; set; } = string.Empty;

        [Required]
        public int ApprovedBy { get; set; }
    }

    public class BulkUserRegistrationStatusRequest
    {
        [Required]
        public List<int> UserIds { get; set; } = new();

        [Required]
        public string RegistrationStatus { get; set; } = string.Empty;

        [Required]
        public int ApprovedBy { get; set; }
    }

    public class UpdateAutoApprovalRequest
    {
        public bool Enabled { get; set; }
    }

    public class CreateUserRequestRequest
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateUserRequestRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
