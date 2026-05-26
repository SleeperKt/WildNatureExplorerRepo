using System.Text.RegularExpressions;

namespace WildNatureExplorer.Application.AI;

/// <summary>Lightweight preprocessing (intent + script) before LLM or tools.</summary>
public static class AiRequestAnalysis
{
    /// <summary>Rough intent bucket for logging and metadata (not a strict classifier).</summary>
    public static string ClassifyIntent(string normalizedQuestion)
    {
        var q = normalizedQuestion;
        if (LooksLikeIdentification(q)) return "species_identification";
        if (LooksLikeConservation(q)) return "conservation";
        if (LooksLikeSafety(q)) return "safety_risk_facts";
        return "general_wildlife";
    }

    /// <summary>Cyrillic vs Latin — requirements mention Cyrillic handling.</summary>
    public static string DetectInputScript(string question)
    {
        var hasCyrillic = question.Any(c => c is >= '\u0400' and <= '\u04FF');
        return hasCyrillic ? "cyrillic" : "latin";
    }

    /// <summary>Normalize once for policy + retrieval + intent.</summary>
    public static string NormalizeWhitespace(string raw)
    {
        return Regex.Replace(raw.Trim(), @"\s+", " ");
    }

    private static bool LooksLikeIdentification(string q)
    {
        return Regex.IsMatch(q, @"\b(what (animal|species)|identify|classification|genus|taxonomy)\b", RegexOptions.IgnoreCase);
    }

    private static bool LooksLikeConservation(string q)
    {
        return Regex.IsMatch(q,
            @"\b(iucn|red list|endangered|extinct|habitat loss|conservation|protected area|poaching)\b",
            RegexOptions.IgnoreCase);
    }

    private static bool LooksLikeSafety(string q)
    {
        return Regex.IsMatch(q,
            @"\b(dangerous|attack|venom|bite|risk to humans|fatal|predator)\b",
            RegexOptions.IgnoreCase);
    }
}
