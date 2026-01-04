using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.AI
{
    public class AiFeedbackDto
    {
        [Required]
        [Description("session rating, Example = 100")]
        public int Rating { get; set; }

        [Required]
        [Description("feedback, Example = AI corectly recognoize image - all good")]
        public string? Comment { get; set; }
    }
}