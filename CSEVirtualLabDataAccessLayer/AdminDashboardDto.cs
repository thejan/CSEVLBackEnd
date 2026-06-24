using System;
using System.Collections.Generic;

namespace CSEVirtualLabDataAccessLayer
{
    public class AdminSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public int Registered { get; set; }
    }

    public class AdminLabCompletionDto
    {
        public int LabId { get; set; }
        public string LabName { get; set; } = string.Empty;
        public int Registered { get; set; }
        public int Completed { get; set; }
    }

    public class AdminUserRegistrationDto
    {
        public int UserId { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Usn { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public byte? Semester { get; set; }
        public string Designation { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string RegistrationStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class AdminUserRequestDto
    {
        public long RequestId { get; set; }
        public int UserId { get; set; }
        public string EmailId { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ClosedAt { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class AdminDashboardDto
    {
        public bool AutoApproveRegistrations { get; set; }
        public List<AdminSummaryDto> CollegeWise { get; set; } = new();
        public List<AdminSummaryDto> DepartmentWise { get; set; } = new();
        public List<AdminSummaryDto> AtmeceDepartmentWise { get; set; } = new();
        public List<AdminLabCompletionDto> CompletionStatus { get; set; } = new();
        public List<AdminUserRegistrationDto> UserRegistrations { get; set; } = new();
        public List<AdminUserRequestDto> UserRequests { get; set; } = new();
    }
}
