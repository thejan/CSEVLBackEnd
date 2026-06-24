namespace CSEVirtualLabWebAPI.Models
{
    public class CProgrammingChatRequest
    {
        public string Message { get; set; } = string.Empty;

        public int ExperimentId { get; set; }

        public string ExperimentTitle { get; set; } = string.Empty;

        public string SourceCode { get; set; } = string.Empty;

        public List<CProgrammingChatMessage> History { get; set; } = [];
    }

    public class CProgrammingChatMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
