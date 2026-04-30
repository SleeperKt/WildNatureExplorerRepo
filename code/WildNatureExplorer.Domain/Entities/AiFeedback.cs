using WildNatureExplorer.Domain.Base;
namespace WildNatureExplorer.Domain.Entities;

public class AiFeedback : Entity
{
    public Guid SessionId { get; set; }
    public AiSession Session { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
