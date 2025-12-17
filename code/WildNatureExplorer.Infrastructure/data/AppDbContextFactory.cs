using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WildNatureExplorer.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Connection string для миграций
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=wild_nature_explorer;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options);
    }
}
