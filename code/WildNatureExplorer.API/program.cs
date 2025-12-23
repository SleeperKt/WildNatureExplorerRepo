using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Infrastructure.Data;
using WildNatureExplorer.Infrastructure.Services;
using WildNatureExplorer.Infrastructure.Repositories;
using Serilog;
using WildNatureExplorer.Infrastructure;
using WildNatureExplorer.Infrastructure.Migrations;
using WildNatureExplorer.Application;
using WildNatureExplorer.Application.DTOs.AI;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

static string Require(IConfiguration config, string key)
{
    return config[key]
        ?? throw new InvalidOperationException($"Configuration value '{key}' is missing");
}

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var dbHost = Require(builder.Configuration, "DB_HOST");
var dbPort = Require(builder.Configuration, "DB_PORT");
var dbName = Require(builder.Configuration, "DB_NAME");
var dbUser = Require(builder.Configuration, "DB_USER");
var dbPassword = Require(builder.Configuration, "DB_PASSWORD");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is not configured");

var connectionString =
    $"Host={dbHost};" +
    $"Port={dbPort};" +
    $"Database={dbName};" +
    $"Username={dbUser};" +
    $"Password={dbPassword}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHttpClient<HuggingFaceVisionService>();
builder.Services.AddHttpClient<GroqChatService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите токен JWT с префиксом Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok());
app.Run();
