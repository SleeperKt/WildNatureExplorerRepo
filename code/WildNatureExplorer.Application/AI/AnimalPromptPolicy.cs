using System.Text.RegularExpressions;

namespace WildNatureExplorer.Application.AI.PromptPolicies;

/// <summary>
/// Wildlife-domain prompt policy applied to the user's typed question
/// before it reaches the LLM.
///
/// <para>
/// <b>Why phrase-based, not bare keywords?</b> The previous version blocked
/// any prompt containing single substrings like <c>kill</c> / <c>weapon</c>.
/// That made the assistant useless for legitimate wildlife topics:
/// <list type="bullet">
/// <item>"Killer whale", "killifish" → blocked.</item>
/// <item>"Can a lion kill a human?" → blocked.</item>
/// <item>Worse: when the AI itself answered something like
///   "Bears can kill humans in self-defence", that text was saved in the
///   conversation history. The next call sent the *full* history to the
///   policy and threw — breaking the dialog forever.</item>
/// </list>
/// We now match on intent <i>phrases</i> (e.g. <c>"how to kill"</c>,
/// <c>"build a bomb"</c>) with whitespace-normalised, case-insensitive
/// substring matching. Wildlife-natural usage passes; explicit harmful
/// instructions are still rejected.
/// </para>
/// </summary>
public static class AnimalPromptPolicy
{
    private static readonly string[] ForbiddenIntentPhrases =
    {
        // Direct harm intents toward animals or humans
        "how to kill",
        "how do i kill",
        "how can i kill",
        "best way to kill",
        "ways to kill",
        "how to harm",
        "how to hurt",
        "how to torture",
        "how to abuse",
        "how to poach",
        "how to poison",
        "how to hunt and kill",

        // Weapons / explosives
        "how to make a bomb",
        "build a bomb",
        "make a bomb",
        "how to make a weapon",
        "build a weapon",
        "make a weapon",

        // Drugs (recreational manufacture / sourcing)
        "how to make drugs",
        "how to cook meth",
        "how to make meth",
        "how to synthesize",
        "where to buy cocaine",
        "where to buy heroin",

        // Hacking / illegal access
        "how to hack",
        "hack into",
        "exploit a vulnerability",
        "ddos attack",
        "how to ddos",
    };

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static void Validate(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            throw new InvalidOperationException(
                "Prompt cannot be empty. Please ask a question about wildlife or nature.");

        // Normalise so phrasing variants ("how   to  kill", "how\tto\nkill")
        // still match the canonical phrase list.
        var normalised = Whitespace.Replace(userPrompt.ToLowerInvariant().Trim(), " ");

        var hit = ForbiddenIntentPhrases.FirstOrDefault(p => normalised.Contains(p));
        if (hit != null)
        {
            throw new InvalidOperationException(
                $"Your question looks like a request for harmful content (matched on \"{hit}\"). " +
                "Please rephrase to focus on wildlife education and conservation.");
        }
    }

    /// <summary>
    /// System prompt for free-form chat. Keeps answers short and visually consistent turn-to-turn.
    /// </summary>
    public static string BuildChatSystemPrompt()
    {
        return """
        You are a wildlife expert AI for Wild Nature Explorer.

        Topics: animals, habitats, ecosystems, conservation, behaviour, diet,
        predator–prey dynamics, danger to humans (factual only), rarity.

        OUTPUT RULES (mandatory):
        - Reply in the same language the user writes in.
        - Total length: aim for under ~90 words; never exceed ~140 words.
        - Use exactly ONE layout every reply:
          • Line 1 — Answer: one clear sentence (max ~35 words).
          • Lines 2–4 — Optional bullets only; each line starts with "• " and is max ~22 words.
          • Never use numbered lists or headings like "###".
        - No greetings ("Hello"), no sign-offs, no "As an AI…".

        Refuse (briefly) harmful instructions: hunting/harming/poaching instructions,
        weapons, drugs, hacking. Redirect to wildlife education.

        You may state factual risk (e.g. attacks on humans) when educational.
        """;
    }

    /// <summary>
    /// System prompt for the post-recognition species blurb (parsed server-side).
    /// </summary>
    public static string BuildStructuredSpeciesSystemPrompt()
    {
        return """
        You write a SHORT factual blurb about ONE recognised animal species for an educational wildlife app.

        CRITICAL — avoid duplication:
        - The **Overview** sentence must ONLY introduce what the animal is (common name + broad group / region). Do NOT mention habitat types, conservation, hunting, danger to humans, or population trends inside Overview.
        - Put habitat ONLY on the **Habitat** bullet.
        - Put risk to people ONLY on the **Risk to humans** bullet (NOT "Danger Level" — that wording was confused with conservation threats).
        - Put wild-population status ONLY on the **Wild population rarity** bullet.

        RULES:
        - English only. Total under ~130 words.
        - Follow EXACTLY this skeleton — nothing before **Overview:**:

        **Overview:** One sentence only (identity / taxonomy / rough geography — no bullets, no Habitat/Risk/Rarity keywords).

        - **Habitat:** Typical biome / region (max ~25 words).
        - **Risk to humans:** Start with Low, Moderate, or High — em-dash — brief reason about physical danger to people from this animal (attacks, venom, size). NOT habitat loss, hunting, or IUCN status here.
        - **Wild population rarity:** Start with Common, Uncommon, Rare, Endangered, or Critically endangered — em-dash — conservation / population note.

        Optional fourth block on its own line (not a bullet): **Interesting fact:** one short sentence.

        Use exactly these bullet labels so downstream code can parse them. No URLs. No extra headings.
        """;
    }

    /// <summary>User turn paired with <see cref="BuildStructuredSpeciesSystemPrompt"/>.</summary>
    public static string BuildStructuredSpeciesUserPrompt(string animalName)
    {
        var safe = string.IsNullOrWhiteSpace(animalName) ? "unknown animal" : animalName.Trim();
        return $"""
            Recogniser label: "{safe}"

            Fill every section from mainstream wildlife knowledge. If the label is ambiguous, stay conservative and say "Uncertain" only inside the relevant bullet text — never inside Overview.
            """;
    }
}
