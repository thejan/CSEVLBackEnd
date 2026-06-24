using System;

namespace CSEVirtualLabDataAccessLayer
{
    public class CompletionCertificateDto
    {
        public int UserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Usn { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public int LabId { get; set; }
        public string LabName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;

        public int TotalExperiments { get; set; }
        public int CompletedExperiments { get; set; }

        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }

        public int TotalQuizScore { get; set; }
        public int TotalQuizMaxMarks { get; set; }
        public double QuizScoreOutOf10 { get; set; }

        public DateTime? CompletionDate { get; set; }
        public bool IsCompleted { get; set; }

        public string CertificateId { get; set; } = string.Empty;
    }
}
