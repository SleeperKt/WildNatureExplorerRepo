using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WildNatureExplorer.Application.DTOs.AI
{
    public class AiQuestionDto
    {
        [Required]
        [Description("Question for the Ai assistant, Example = What is the largest animal in the world?")]
        [JsonPropertyName("questionAboutNature")]
        public string? QuestionAboutNature { get; set; }
    }
}
