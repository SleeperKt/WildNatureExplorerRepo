using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class DomainAiEntitiesSmokeTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Ai_session_graph_properties_roundtrip()
    {
        var session = new AiSession
        {
            UserId = Guid.NewGuid(),
            AnimalName = "Boar",
            ImageUrl = "https://example.test/i.jpg",
            IsEnded = true,
            EndedAt = DateTime.UtcNow
        };

        session.Messages.Add(new AiMessage
        {
            SessionId = session.Id,
            Role = "User",
            Content = "Hello",
            Session = session
        });

        Assert.Single(session.Messages);

        var fb = new AiFeedback
        {
            SessionId = session.Id,
            Session = session,
            Rating = 90,
            Comment = "nice"
        };

        Assert.Equal(90, fb.Rating);
    }
}
