using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using System.Text.RegularExpressions;
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
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

static string Require(IConfiguration config, string key)
{
    return config[key]
        ?? throw new InvalidOperationException($"Configuration value '{key}' is missing");
}

static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

static string BuildPostgreSqlConnectionString(string host, string port, string database, string username, string password) =>
    $"Host={host};" +
    $"Port={port};" +
    $"Database={database};" +
    $"Username={username};" +
    $"Password={password}";

static async Task EnsureApplicationDbRoleCanLoginAsync(
    string ownerConnectionString,
    string roleName,
    string rolePassword)
{
    if (!Regex.IsMatch(roleName, "^[a-z][a-z0-9_]{0,62}$"))
    {
        throw new InvalidOperationException(
            "Configured application DB role name must match ^[a-z][a-z0-9_]*$.");
    }

    await using var conn = new NpgsqlConnection(ownerConnectionString);
    await conn.OpenAsync();

    await using (var alter = conn.CreateCommand())
    {
        alter.CommandText =
            $"ALTER ROLE {roleName} WITH LOGIN PASSWORD @pw";
        alter.Parameters.AddWithValue("pw", rolePassword);
        await alter.ExecuteNonQueryAsync();
    }

    await using (var connect = conn.CreateCommand())
    {
        connect.CommandText =
            $"""
             DO $$
             BEGIN
               EXECUTE format('GRANT CONNECT ON DATABASE %I TO %I', current_database(), '{roleName}');
             END $$;
             """;
        await connect.ExecuteNonQueryAsync();
    }
}

var builder = WebApplication.CreateBuilder(args);

var skipHeavyStartupForOpenApi =
    string.Equals(Environment.GetEnvironmentVariable("WNE_OPENAPI_GEN"), "1", StringComparison.Ordinal);

var corsOriginsRaw = builder.Configuration["CORS_ORIGINS"];
string[] corsOrigins;
if (string.IsNullOrWhiteSpace(corsOriginsRaw))
{
    corsOrigins =
    [
        "http://localhost:5173",
        "http://localhost:5174",
        "http://127.0.0.1:5173",
        "http://127.0.0.1:5174"
    ];
}
else
{
    corsOrigins = corsOriginsRaw.Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Host.UseSerilog();

var dbHost = Require(builder.Configuration, "DB_HOST");
var dbPort = Require(builder.Configuration, "DB_PORT");
var dbName = Require(builder.Configuration, "DB_NAME");
var dbUser = Require(builder.Configuration, "DB_USER");
var dbPassword = Require(builder.Configuration, "DB_PASSWORD");

var migrateUser = NonEmpty(builder.Configuration["DB_MIGRATE_USER"]) ?? dbUser;
var migratePassword = NonEmpty(builder.Configuration["DB_MIGRATE_PASSWORD"]) ?? dbPassword;

var jwtKey = Require(builder.Configuration, "JWT_KEY");
var jwtIssuer = Require(builder.Configuration, "JWT_ISSUER");
var jwtAudience = Require(builder.Configuration, "JWT_AUDIENCE");

var applicationConnectionString = BuildPostgreSqlConnectionString(dbHost, dbPort, dbName, dbUser, dbPassword);
var migrateConnectionString =
    BuildPostgreSqlConnectionString(dbHost, dbPort, dbName, migrateUser, migratePassword);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(applicationConnectionString));

builder.Services.AddValidatorsFromAssemblyContaining<AdminSpeciesImportDto>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHttpClient<HuggingFaceVisionService>();
builder.Services.AddHttpClient<AnimalDetectVisionService>();
builder.Services.AddHttpClient<GroqChatService>();

builder.Services.Configure<ChatLlmOptions>(builder.Configuration.GetSection(ChatLlmOptions.SectionName));
builder.Services.Configure<AiInferenceOptions>(builder.Configuration.GetSection(AiInferenceOptions.SectionName));
builder.Services.Configure<AiKnowledgeOptions>(builder.Configuration.GetSection(AiKnowledgeOptions.SectionName));
builder.Services.Configure<AiRateLimitOptions>(builder.Configuration.GetSection(AiRateLimitOptions.SectionName));
builder.Services.AddSingleton<WildlifeKnowledgeRetriever>();

var aiRateParsed = builder.Configuration.GetSection(AiRateLimitOptions.SectionName).Get<AiRateLimitOptions>()
                     ?? new AiRateLimitOptions();

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.HttpContext.Response.WriteAsync(
            "Too many AI requests for this window. Pause briefly and retry.",
            token);
    };

    options.AddPolicy("AiEndpoints", httpContext =>
    {
        var userKey = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(
            userKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(aiRateParsed.PermitLimit, 1),
                Window = TimeSpan.FromMinutes(Math.Max(aiRateParsed.WindowMinutes, 1)),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 8
            });
    });
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminImportService, AdminImportService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPathSimulationService, PathSimulationService>();
builder.Services.AddScoped<IUserLibraryService, UserLibraryService>();

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
builder.Services.AddScoped<IUserSightingRepository, UserSightingRepository>();

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
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Wild Nature Explorer API",
            Version = "v1",
            Description =
                "Wild Nature Explorer REST API: JWT auth, species catalogue, maps, user library, admin import, AI endpoints."
        });

    c.AddServer(
        new OpenApiServer
        {
            Url = "http://localhost:5000",
            Description = "Local / Docker Compose."
        });

    c.AddServer(
        new OpenApiServer
        {
            Url = "https://wildnatureexplorerapi-e2a6hpc4gah0ceb5.italynorth-01.azurewebsites.net",
            Description = "Production — Azure App Service."
        });

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

if (!skipHeavyStartupForOpenApi)
{
    await using var migrateDb = new AppDbContext(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(migrateConnectionString)
            .Options);

    await migrateDb.Database.MigrateAsync();

    if (!migrateUser.Equals(dbUser, StringComparison.Ordinal))
        await EnsureApplicationDbRoleCanLoginAsync(migrateConnectionString, dbUser, dbPassword);
}

if (!skipHeavyStartupForOpenApi)
{
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
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TermsMiddleware>();

app.MapGet("/health", () => Results.Ok());
app.MapControllers();

app.Run();