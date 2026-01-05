using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data.Configurations;

namespace WildNatureExplorer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AiSession> AiSessions => Set<AiSession>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<AiFeedback> AiFeedbacks => Set<AiFeedback>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Habitat> Habitats => Set<Habitat>();
    public DbSet<Size> Sizes => Set<Size>();

    public DbSet<SpeciesCountry> SpeciesCountries => Set<SpeciesCountry>();
    public DbSet<SpeciesColor> SpeciesColors => Set<SpeciesColor>();
    public DbSet<SpeciesHabitat> SpeciesHabitats => Set<SpeciesHabitat>();

    public DbSet<SpeciesLocation> SpeciesLocations => Set<SpeciesLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
