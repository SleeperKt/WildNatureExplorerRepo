namespace WildNatureExplorer.Application.DTOs.AI;

public class AiTechnicalResponse
{
    public double QueueTime { get; set; }
    public double PromptTime { get; set; }
    public double CompletionTime { get; set; }
    public double TotalTime { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public string SystemFingerprint { get; set; }
    public string RequestId { get; set; }
}
