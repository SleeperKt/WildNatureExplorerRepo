namespace WildNatureExplorer.Application.AI.PromptPolicies;

public static class AnimalPromptPolicy
{
    private static readonly string[] ForbiddenKeywords =
    {
        "weapon",
        "violence",
        "illegal",
        "drug",
        "bomb",
        "kill",
        "exploit",
        "hack"
    };

    public static void Validate(string userPrompt)
    {
        var lower = userPrompt.ToLowerInvariant();

        if (ForbiddenKeywords.Any(k => lower.Contains(k)))
            throw new InvalidOperationException("Prompt violates AI safety policy.");
    }

    public static string BuildSystemPrompt()
    {
        return """
        You are a wildlife expert AI.
        You only answer questions related to animals, habitats,
        conservation, danger levels, rarity, and comparisons.
        You must refuse unrelated topics.
        Responses must be factual and concise.
        """;
    }
}
