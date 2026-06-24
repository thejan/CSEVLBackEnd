namespace CSEVirtualLabWebAPI.Models
{
    public class CodeExecutionRequest
    {
        public string SourceCode { get; set; } = "";

        public string Input { get; set; } = "";
    }
}