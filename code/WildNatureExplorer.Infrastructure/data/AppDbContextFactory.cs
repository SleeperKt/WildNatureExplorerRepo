using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WildNatureExplorer.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Берём переменные окружения
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var db = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");

        // Проверка, что все переменные заданы
        // if (string.IsNullOrWhiteSpace(host) ||
        //     string.IsNullOrWhiteSpace(port) ||
        //     string.IsNullOrWhiteSpace(db) ||
        //     string.IsNullOrWhiteSpace(user) ||
        //     string.IsNullOrWhiteSpace(pass))
        // {
        //    var connectionString = "Host=localhost;Port=5432;Database=wild_nature_explorer;Username=postgres;Password=postgres";
        // }
        // else
        // {
        //    var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
        // }
        var connectionString = "Host=localhost;Port=5432;Database=wild_nature_explorer;Username=postgres;Password=postgres";

        // Настраиваем DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
