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
using FluentValidation.AspNetCore;
using WildNatureExplorer.API.Middlewares;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WildNatureExplorer.Application.DTOs.Admin;
using FluentValidation;
using WildNatureExplorer.Domain.Entities;
using System.Security.Claims;

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


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://fragile-arrival-viselike.ngrok-free.dev"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Host.UseSerilog();

var dbHost = Require(builder.Configuration, "DB_HOST");
var dbPort = Require(builder.Configuration, "DB_PORT");
var dbName = Require(builder.Configuration, "DB_NAME");
var dbUser = Require(builder.Configuration, "DB_USER");
var dbPassword = Require(builder.Configuration, "DB_PASSWORD");

var jwtKey = Require(builder.Configuration, "JWT_KEY");
var jwtIssuer = Require(builder.Configuration, "JWT_ISSUER");
var jwtAudience = Require(builder.Configuration, "JWT_AUDIENCE");

var connectionString =
    $"Host={dbHost};" +
    $"Port={dbPort};" +
    $"Database={dbName};" +
    $"Username={dbUser};" +
    $"Password={dbPassword}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<AdminSpeciesImportDto>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHttpClient<HuggingFaceVisionService>();
builder.Services.AddHttpClient<GroqChatService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminImportService, AdminImportService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<IHabitatRepository, HabitatRepository>();
builder.Services.AddScoped<ISpeciesLocationRepository, SpeciesLocationRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role,
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
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations(
        enableAnnotationsForInheritance: false,
        enableAnnotationsForPolymorphism: false
    );
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
    var roles = new[]
    {
        ("User", "Default role for all registered users"),
        ("Admin", "Administrator with full privileges"),
        ("Moderator", "Moderator role assigned by Admin")
    };

    foreach (var (name, desc) in roles)
    {
        var existing = await roleRepo.GetByNameAsync(name);
        if (existing == null)
        {
            var role = new Role(Guid.NewGuid(), name, desc);
            await roleRepo.AddAsync(role);
        }
    }
}

app.UseCors();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok());
app.Run();
