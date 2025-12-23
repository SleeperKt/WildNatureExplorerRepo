using WildNatureExplorer.Domain.Base;
namespace WildNatureExplorer.Domain.Entities;

public class AiSession : Entity
{
    public Guid UserId { get; set; }
    public string AnimalName { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}
