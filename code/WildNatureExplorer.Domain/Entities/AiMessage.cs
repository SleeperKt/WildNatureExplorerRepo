using WildNatureExplorer.Domain.Base;
namespace WildNatureExplorer.Domain.Entities;

public class AiMessage : Entity
{

    public Guid SessionId { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiSession Session { get; set; } = null!;
}
