namespace WildNatureExplorer.Application.DTOs.AI;

/// <summary>Outcome of a Groq chat completion (possibly after fallback retry).</summary>
public class LlmChatResult
{
    public string Content { get; set; } = string.Empty;
    public UsageDto Usage { get; set; } = new();
    public string ModelUsed { get; set; } = string.Empty;
    public bool UsedFallbackModel { get; set; }
}
