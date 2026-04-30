using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WildNatureExplorer.Application.Options;

namespace WildNatureExplorer.Infrastructure.Services;

/// <summary>
/// Keyword overlap retrieval over a static JSON corpus (RAG baseline without embeddings).
/// </summary>
public sealed class WildlifeKnowledgeRetriever
{
    private readonly ILogger<WildlifeKnowledgeRetriever> _logger;
    private readonly AiKnowledgeOptions _opts;
    private KnowledgeChunk[] _chunks = Array.Empty<KnowledgeChunk>();

    public WildlifeKnowledgeRetriever(IOptions<AiKnowledgeOptions> opts, ILogger<WildlifeKnowledgeRetriever> logger)
    {
        _opts = opts.Value;
        _logger = logger;
        TryLoad();
    }

    public (string Snippet, IReadOnlyList<string> ChunkIds) Retrieve(string question)
    {
        if (_chunks.Length == 0 || string.IsNullOrWhiteSpace(question))
            return (string.Empty, Array.Empty<string>());

        var qTokens = Words(question);
        if (qTokens.Count == 0)
            return (string.Empty, Array.Empty<string>());

        var scored = _chunks
            .Select(c => (Chunk: c, Score: Score(qTokens, c)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(4)
            .ToList();

        if (scored.Count == 0)
            return (string.Empty, Array.Empty<string>());

        var ids = new List<string>();
        var sb = new System.Text.StringBuilder();
        var budget = _opts.MaxInjectedContextChars;
        foreach (var row in scored)
        {
            var block = $"[{row.Chunk.Id}] {row.Chunk.Title}: {row.Chunk.Text}";
            if (sb.Length + block.Length + 2 > budget)
                break;
            if (sb.Length > 0) sb.AppendLine();
            sb.Append(block);
            ids.Add(row.Chunk.Id);
        }

        return (sb.ToString(), ids);
    }

    private static HashSet<string> Words(string s)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in Regex.Matches(s.ToLowerInvariant(), @"[\p{L}\p{Nd}]{3,}"))
            set.Add(m.Value);
        return set;
    }

    private static int Score(HashSet<string> query, KnowledgeChunk c)
    {
        var title = Words(c.Title ?? string.Empty);
        var body = Words($"{c.Title} {c.Text}");
        var tags = Words(c.Tags != null ? string.Join(' ', c.Tags) : string.Empty);
        return Overlap(query, title) * 4 + Overlap(query, body) * 2 + Overlap(query, tags) * 3;

        static int Overlap(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0;
            var n = 0;
            foreach (var w in a)
            {
                if (b.Contains(w)) n++;
            }

            return n;
        }
    }

    private void TryLoad()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, _opts.ChunksFileRelativePath);
            if (!File.Exists(path))
            {
                _logger.LogWarning("Knowledge corpus not found at {Path}. RAG injection disabled.", path);
                return;
            }

            var json = File.ReadAllText(path);
            var chunks = JsonSerializer.Deserialize<KnowledgeChunk[]>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _chunks = chunks ?? Array.Empty<KnowledgeChunk>();
            _logger.LogInformation("Loaded {Count} knowledge chunks.", _chunks.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load knowledge corpus.");
        }
    }

    private sealed class KnowledgeChunk
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public string[]? Tags { get; set; }
    }
}
