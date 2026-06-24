namespace CSEVirtualLabDataAccessLayer
{
    public class QuizAttemptDto
    {
        public bool HasAttempted { get; set; }

        public int? QuizScore { get; set; }

        public int? QuizMaxMarks { get; set; }

        public DateTime? SubmittedAt { get; set; }
    }
}
