namespace WildNatureExplorer.Application.AI;

/// <summary>Minimal post-processing so clients can safely render bullets without broken markdown.</summary>
public static class AiMarkdownFormatter
{
    public static string NormalizeForMarkdown(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var lines = raw
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.None)
            .Select(l => l.TrimEnd());

        var compact = string.Join('\n', lines).Trim();

        while (compact.Contains("\n\n\n"))
            compact = compact.Replace("\n\n\n", "\n\n");

        return compact;
    }
}
