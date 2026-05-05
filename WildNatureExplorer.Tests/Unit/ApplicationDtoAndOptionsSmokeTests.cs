using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.DTOs.AI;
using WildNatureExplorer.Application.DTOs.Geo;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.DTOs.Species;
using WildNatureExplorer.Application.Options;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

/// <summary>Executes constructors/setters on transport types so coverage reflects API contracts.</summary>
public class ApplicationDtoAndOptionsSmokeTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Ai_and_geo_payload_graphs_are_assignable()
    {
        var sessionId = Guid.NewGuid();

        var resp = new AiSessionResponseDto
        {
            SessionId = sessionId,
            Answer = "Hi",
            Animal = new AnimalInfoDto { Name = "Bear", Habitat = "Forest" },
            Technical = new TechnicalInfoDto
            {
                Inference = new InferenceMetadataDto
                {
                    Intent = "general_wildlife",
                    RetrievalChunkIds = new[] { "c1" },
                    ToolsUsed = Array.Empty<string>()
                },
                Usage = new UsageDto { TotalTokens = 3, PromptTokens = 1, CompletionTokens = 2 }
            },
            EndedAt = DateTime.UtcNow
        };
        Assert.Equal(sessionId, resp.SessionId);
        Assert.Equal("Bear", resp.Animal!.Name);
        Assert.Equal("general_wildlife", resp.Technical.Inference!.Intent);

        var chat = new ChatResponseDto { Answer = "x", SessionId = sessionId, AnswerMarkdown = "*x*" };
        Assert.Equal(0, chat.Technical.Usage.TotalTokens);

        var analysis = new AnimalAnalysisResponseDto { SessionId = sessionId };
        analysis.Animal.Name = "Fox";
        analysis.Technical.Usage.TotalTokens = 10;
        Assert.Equal("Fox", analysis.Animal.Name);

        var llm = new LlmChatResult { Content = "ok", ModelUsed = "m", UsedFallbackModel = true };

        Assert.Equal("ok", llm.Content);
        Assert.True(llm.UsedFallbackModel);
        Assert.NotNull(llm.Usage);

        var req = new PathSimulationRequest
        {
            CountryId = Guid.NewGuid(),
            Waypoints = new List<WaypointDto> { new() { Lat = 1, Lng = 2 } },
            StepsPerSegment = 5,
            DangerRadiusKm = 3
        };
        var stepResp = new PathSimulationResponse
        {
            PathSteps = new List<SimulationStepDto>
            {
                new()
                {
                    StepId = 1,
                    SegmentId = 0,
                    Latitude = 1,
                    Longitude = 2,
                    TimestampStep = DateTime.UtcNow,
                    DistanceFromStartKm = 0
                }
            },
            Alerts = new List<DangerAlertDto>
            {
                new()
                {
                    AlertId = Guid.NewGuid(),
                    StepId = 1,
                    AnimalId = Guid.NewGuid(),
                    AnimalName = "Wolf",
                    WarningLevel = "WARNING"
                }
            },
            TotalSteps = 1,
            TotalAlerts = 1,
            TotalDistanceKm = 2,
            SimulatedAt = DateTime.UtcNow
        };

        Assert.Single(req.Waypoints);
        Assert.Equal(1, stepResp.PathSteps[0].StepId);
        Assert.Equal("Wolf", stepResp.Alerts[0].AnimalName);

        var nearby = new NearbySightingResponse { DistanceKm = 12.5, CommonName = "Elk", Latitude = 1, Longitude = 2 };
        Assert.Equal(12.5, nearby.DistanceKm);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Species_records_and_admin_csv_metadata_construct()
    {
        var id = Guid.NewGuid();
        var details = new SpeciesDetailsDto(
            id,
            "Lynx",
            "Lynx lynx",
            "desc",
            true,
            false,
            "Medium",
            new List<string> { "Tan" },
            new List<string> { "Forest" },
            new List<string> { "Norway" });

        var shortDto = new SpeciesShortDto(id, "Lynx", true, false);

        Assert.Equal(details.CommonName, shortDto.CommonName);
        Assert.Equal("Medium", details.Size);
        Assert.Single(details.Colors);

        using var ms = new MemoryStream();
        var csvDto = new AdminSpeciesLocationsCsvDto
        {
            SpeciesId = id,
            FileStream = ms,
            FileName = "locs.csv"
        };
        Assert.True(csvDto.FileStream.CanRead);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Ai_configuration_sections_have_defaults()
    {
        Assert.False(string.IsNullOrEmpty(ChatLlmOptions.SectionName));

        var chat = new ChatLlmOptions { PrimaryModel = "x", FallbackModel = "y", ApiKey = "k", ChatTemperature = 0.5 };
        Assert.True(chat.ChatMaxTokens > 0);

        Assert.True(new AiInferenceOptions().MaxUserMessageChars > 0);
        Assert.False(string.IsNullOrEmpty(new AiKnowledgeOptions().ChunksFileRelativePath));
        Assert.True(new AiRateLimitOptions().PermitLimit > 0);
    }
}
