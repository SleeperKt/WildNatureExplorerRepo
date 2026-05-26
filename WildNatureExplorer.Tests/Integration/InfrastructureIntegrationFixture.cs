using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Tests.Integration;

internal static class InfrastructureIntegrationFixture
{
    internal static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
