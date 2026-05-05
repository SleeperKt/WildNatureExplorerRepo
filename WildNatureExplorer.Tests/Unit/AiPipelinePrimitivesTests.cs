using WildNatureExplorer.Application.AI;
using WildNatureExplorer.Application.Common;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class AiPipelinePrimitivesTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void AiMarkdownFormatter_NormalizeForMarkdown_TrimsAndCollapsesBlankRuns()
    {
        Assert.Equal(string.Empty, AiMarkdownFormatter.NormalizeForMarkdown(null!));
        Assert.Equal(string.Empty, AiMarkdownFormatter.NormalizeForMarkdown("   "));

        var raw = " a \r\n\r\n\r\n b ";
        Assert.Equal("a\n\n b", AiMarkdownFormatter.NormalizeForMarkdown(raw));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AiRequestAnalysis_ClassifyIntent_BucketsKeywords()
    {
        Assert.Equal("species_identification", AiRequestAnalysis.ClassifyIntent("what animal is this"));
        Assert.Equal("conservation", AiRequestAnalysis.ClassifyIntent("IUCN red list status"));
        Assert.Equal("safety_risk_facts", AiRequestAnalysis.ClassifyIntent("is it dangerous to humans"));
        Assert.Equal("general_wildlife", AiRequestAnalysis.ClassifyIntent("tell me about moss"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AiRequestAnalysis_DetectInputScript_DistinguishesScripts()
    {
        Assert.Equal("cyrillic", AiRequestAnalysis.DetectInputScript("Что это за животное?"));
        Assert.Equal("latin", AiRequestAnalysis.DetectInputScript("What animal is this?"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AiRequestAnalysis_NormalizeWhitespace_CompressesRuns()
    {
        Assert.Equal("one two", AiRequestAnalysis.NormalizeWhitespace("  one\t two\n "));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InvalidAiSessionException_RoundTripMessage()
    {
        var ex = new InvalidAiSessionException("gone");
        Assert.Equal("gone", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SafetyPolicyException_Constructors()
    {
        var ex = new SafetyPolicyException("policy");
        Assert.Equal("policy", ex.Message);

        var inner = new InvalidOperationException("inner");
        var wrapped = new SafetyPolicyException("outer", inner);
        Assert.Same(inner, wrapped.InnerException);
    }
}
