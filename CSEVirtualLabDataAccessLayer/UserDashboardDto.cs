using System;
using System.Collections.Generic;
using System.Text;

namespace CSEVirtualLabDataAccessLayer
{
    public class UserDashboardDto
    {
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Usn { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public byte Semester { get; set; }
        public int RegisteredLabCount { get; set; }

        public List<UserLabDashboardDto> Labs { get; set; } = new();
    }

    public class UserLabDashboardDto
    {
        public int LabId { get; set; }
        public string LabName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string EnrollmentStatus { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public bool CanDownloadCertificate { get; set; }
        public int ProgressPercentage { get; set; }

        public List<ExperimentProgressDto> Experiments { get; set; } =
            new();
    }

    public class ExperimentProgressDto
    {
        public int ExperimentId { get; set; }
        public byte ExperimentNumber { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
        public string Execution { get; set; } = string.Empty;
        public string Quiz { get; set; } = string.Empty;
        public string Assignments { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
