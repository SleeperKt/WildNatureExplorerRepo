namespace WildNatureExplorer.Application.DTOs.AI
{
    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public TechnicalInfoDto Technical { get; set; } = new TechnicalInfoDto();
        public Guid SessionId { get; set; }

        /// <summary>Post-processed for clients that render Markdown (optional).</summary>
        public string? AnswerMarkdown { get; set; }
    }
}