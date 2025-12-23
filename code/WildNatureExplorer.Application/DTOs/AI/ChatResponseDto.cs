using System.Text.Json.Serialization;

namespace WildNatureExplorer.Application.DTOs.AI
{
    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public TechnicalInfoDto Technical { get; set; } = null!;
    }
}