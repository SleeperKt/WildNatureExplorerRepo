namespace WildNatureExplorer.Application.DTOs.AI
{
    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public TechnicalInfoDto Technical { get; set; } = new TechnicalInfoDto();
        public Guid SessionId { get; set; }
    }
}