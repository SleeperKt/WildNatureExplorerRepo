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
        if (string.IsNullOrWhiteSpace(userPrompt))
            throw new InvalidOperationException("Prompt cannot be empty. Please ask a question about wildlife or nature.");

        var lower = userPrompt.ToLowerInvariant();

        var violatingKeyword = ForbiddenKeywords.FirstOrDefault(k => lower.Contains(k));
        
        if (violatingKeyword != null)
            throw new InvalidOperationException($"Prompt contains forbidden keyword: '{violatingKeyword}'. Please rephrase your question to focus on wildlife education and conservation.");
    }

    public static string BuildSystemPrompt()
    {
        return """
        You are a wildlife expert AI assistant for the Wild Nature Explorer app.
        You ONLY answer questions related to animals, habitats, conservation, danger levels, rarity, size, diet, behavior, and comparisons.
        You must refuse any questions about:
        - Weapons, violence, or illegal activities
        - Drugs or harmful substances
        - How to hunt or harm animals
        - Hacking or unauthorized access
        
        For out-of-scope topics, politely redirect to wildlife topics.
        Responses must be factual, concise, and educational.
        Focus on interesting facts that would appeal to wildlife enthusiasts.
        """;
    }
}
