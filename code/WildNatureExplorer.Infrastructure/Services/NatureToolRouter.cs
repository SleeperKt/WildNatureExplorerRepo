using System.Text.RegularExpressions;
using WildNatureExplorer.Application.AI;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.Infrastructure.Services;

/// <summary>
/// Three lightweight server-side tools invoked before the LLM when patterns match.
/// This is not full model \"function calling\" — it demonstrates tool routing + cost control.
/// </summary>
public static class NatureToolRouter
{
    /// <summary>
    /// Answers without LLM when a simple deterministic tool applies.
    /// </summary>
    public static bool TryResolve(string question, out ChatResponseDto dto)
    {
        dto = default!;
        var raw = question?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(raw))
            return false;

        if (TryUtcTime(raw, out var utc))
        {
            dto = ToolResponse(raw, utc,
                new[] { "tool_get_utc_iso" });
            return true;
        }

        if (TryIucnTigerStub(raw, out var tiger))
        {
            dto = ToolResponse(raw, tiger,
                new[] { "tool_iucn_tiger_stub" });
            return true;
        }

        if (TryProjectHelp(raw, out var help))
        {
            dto = ToolResponse(raw, help,
                new[] { "tool_wne_help" });
            return true;
        }

        return false;
    }

    private static bool TryUtcTime(string q, out string message)
    {
        message = "";
        if (!Regex.IsMatch(q, @"^(what('?s| is) the (current )?time|(current )?utc( time)?)[\s!.?]*$",
                RegexOptions.IgnoreCase))
            return false;
        message = $"Answer: Current server time is {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.\n\n• You can ask wildlife questions in the same chat — I will keep Cyrillic and Latin text.";
        return true;
    }

    private static bool TryIucnTigerStub(string q, out string message)
    {
        message = "";
        if (!Regex.IsMatch(q, @"(?i)\b(tiger|panthera tigris|iucn|red list).*", RegexOptions.None))
            return false;
        if (!Regex.IsMatch(q, @"(?i)\b(iucn|red list|endangered|conservation status)\b"))
            return false;

        message =
            "Answer: Tiger (_Panthera tigris_) is a large felid; status is often cited as Endangered globally (verify on the IUCN Red List site).\n\n" +
            "• Threats: habitat loss, poaching, fragmentation.\n" +
            "• Public safety: wild tigers avoid humans but can be dangerous when surprised or cornered.";
        return true;
    }

    private static bool TryProjectHelp(string q, out string message)
    {
        message = "";
        if (!Regex.IsMatch(q, @"^(help|how does this work|what can you do)[\s!.?]*$", RegexOptions.IgnoreCase))
            return false;

        message =
            "Answer: I discuss animals, habitats, and conservation. Upload a photo to analyse it, then ask follow-up questions.\n\n" +
            "• Use the recognizer dropdown to switch vision backends.\n" +
            "• Start \"New chat\" to reset context.\n" +
            "• I refuse harmful instructions (weapons, harming wildlife, illegal activity).";
        return true;
    }

    private static ChatResponseDto ToolResponse(string originalQuestion, string answer,
        IReadOnlyList<string> tools)
    {
        var norm = AiRequestAnalysis.NormalizeWhitespace(originalQuestion);

        return new ChatResponseDto
        {
            Answer = answer,
            AnswerMarkdown = AiMarkdownFormatter.NormalizeForMarkdown(answer),
            Technical = new TechnicalInfoDto
            {
                Usage = new UsageDto(),
                Inference = new InferenceMetadataDto
                {
                    EffectiveModel = "(no LLM — tool)",
                    PrimaryModel = "(none)",
                    LlmSkipped = true,
                    ToolsUsed = tools,
                    RetrievalChunkIds = Array.Empty<string>(),
                    Intent = AiRequestAnalysis.ClassifyIntent(norm),
                    DetectedInputScript = AiRequestAnalysis.DetectInputScript(originalQuestion)
                }
            }
        };
    }
}
