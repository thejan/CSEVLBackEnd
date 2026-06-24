using System;
using System.Collections.Generic;

namespace CSEVirtualLabDataAccessLayer
{
    public class LabReportDto
    {
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Usn { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public byte Semester { get; set; }
        public int LabId { get; set; }
        public string LabName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string EnrollmentStatus { get; set; } = string.Empty;
        public DateTime? DateOfRegistration { get; set; }
        public DateTime? DateOfCompletion { get; set; }
        public bool CanDownloadCertificate { get; set; }
        public double AverageQuizScore { get; set; }

        public List<LabReportExperimentDto> Experiments { get; set; } =
            new();
    }

    public class LabReportExperimentDto
    {
        public int ExperimentId { get; set; }
        public byte ExperimentNumber { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int PartExperimentNumber { get; set; }
        public string ExperimentTitle { get; set; } = string.Empty;
        public string Execution { get; set; } = string.Empty;
        public string Quiz { get; set; } = string.Empty;
        public string Assignments { get; set; } = string.Empty;
        public string CompletionStatus { get; set; } = string.Empty;
    }
}
