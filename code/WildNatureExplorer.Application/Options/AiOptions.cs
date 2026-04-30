namespace WildNatureExplorer.Application.Options;

public class ChatLlmOptions
{
    public const string SectionName = "ChatLlm";

    /// <summary>Groq endpoint (OpenAI-compatible).</summary>
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";

    /// <summary>
    /// Prefer environment variable GROQ_API_KEY when this is empty.
    /// </summary>
    public string? ApiKey { get; set; }

    public string PrimaryModel { get; set; } = "llama-3.1-8b-instant";

    /// <summary>Used when the primary model returns 429/503/timeout-style failures.</summary>
    public string FallbackModel { get; set; } = "llama-3.1-8b-instant";

    public double ChatTemperature { get; set; } = 0.38;
    public int ChatMaxTokens { get; set; } = 320;
    public double StructuredTemperature { get; set; } = 0.28;
    public int StructuredMaxTokens { get; set; } = 450;

    /// <summary>Timeout for a single completion call (seconds).</summary>
    public int RequestTimeoutSeconds { get; set; } = 55;
}

public class AiInferenceOptions
{
    public const string SectionName = "AiInference";

    /// <summary>Hard cap on incoming user message characters (Cyrillic-safe).</summary>
    public int MaxUserMessageChars { get; set; } = 4096;
}

public class AiKnowledgeOptions
{
    public const string SectionName = "AiKnowledge";

    /// <summary>Relative to application base directory (copy to output).</summary>
    public string ChunksFileRelativePath { get; set; } = "docs/ai/knowledge_chunks.json";

    /// <summary>Max chars of retrieved context injected into the system prompt.</summary>
    public int MaxInjectedContextChars { get; set; } = 1400;
}

public class AiRateLimitOptions
{
    public const string SectionName = "AiRateLimit";

    /// <summary>Max AI requests per authenticated user per sliding window.</summary>
    public int PermitLimit { get; set; } = 60;

    /// <summary>Sliding window duration in minutes.</summary>
    public int WindowMinutes { get; set; } = 1;

    /// <summary>How many segments the window is divided into (higher = smoother).</summary>
    public int SegmentsPerWindow { get; set; } = 6;
}
