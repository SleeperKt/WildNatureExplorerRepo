using WildNatureExplorer.Application.AI.PromptPolicies;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class AnimalPromptPolicyTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_EmptyPrompt_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AnimalPromptPolicy.Validate("   "));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_HarmfulPhrase_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AnimalPromptPolicy.Validate("How to kill wolves for sport?"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_LegitimateWildlifeQuestion_DoesNotThrow()
    {
        AnimalPromptPolicy.Validate("Can a lion swim?");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildChatSystemPrompt_ReturnsMarkdownRules()
    {
        var prompt = AnimalPromptPolicy.BuildChatSystemPrompt();
        Assert.Contains("wildlife", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildStructuredSpeciesPrompts_NonEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AnimalPromptPolicy.BuildStructuredSpeciesSystemPrompt()));
        Assert.Contains("Overview", AnimalPromptPolicy.BuildStructuredSpeciesUserPrompt("Tiger"));
    }
}
