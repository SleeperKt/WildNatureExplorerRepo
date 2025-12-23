using System.Text.Json.Serialization;

namespace WildNatureExplorer.Application.DTOs.AI
{
    public class AnimalAnalysisResponseDto
    {
        public AnimalInfoDto Animal { get; set; } = new AnimalInfoDto();
        public TechnicalInfoDto Technical { get; set; } = new TechnicalInfoDto();
        public Guid SessionId { get; set; }
    }

    public class AnimalInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Habitat { get; set; } = string.Empty;
        public string DangerLevel { get; set; } = string.Empty;
        public string RarityLevel { get; set; } = string.Empty;
    }

    public class TechnicalInfoDto
    {
        public UsageDto Usage { get; set; } = new UsageDto();
    }

    public class UsageDto
    {
        [JsonPropertyName("queue_time")]
        public double QueueTime { get; set; }

        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("total_time")]
        public double TotalTime { get; set; }
    }
}