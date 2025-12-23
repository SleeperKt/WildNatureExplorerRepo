using WildNatureExplorer.Domain.Base;
namespace WildNatureExplorer.Domain.Entities;

public class AiFeedback : Entity
{
    public Guid SessionId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
