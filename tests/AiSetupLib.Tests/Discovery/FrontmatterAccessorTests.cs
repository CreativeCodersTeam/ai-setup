using AiSetup.Discovery;
using AiSetup.Models;

namespace AiSetup.Tests.Discovery;

public sealed class FrontmatterAccessorTests
{
    [Fact]
    public void GetString_MissingKey_ReturnsNull()
    {
        var dict = new Dictionary<string, object?>();

        dict.GetString("name").Should().BeNull();
    }

    [Fact]
    public void GetString_StringValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object?> { ["name"] = "foo" };

        dict.GetString("name").Should().Be("foo");
    }

    [Fact]
    public void GetString_NullValue_ReturnsNull()
    {
        var dict = new Dictionary<string, object?> { ["name"] = null };

        dict.GetString("name").Should().BeNull();
    }

    [Fact]
    public void GetString_NonStringValue_UsesToString()
    {
        var dict = new Dictionary<string, object?> { ["count"] = 42 };

        dict.GetString("count").Should().Be("42");
    }

    [Fact]
    public void GetStringList_MissingKey_ReturnsEmpty()
    {
        var dict = new Dictionary<string, object?>();

        dict.GetStringList("tags").Should().BeEmpty();
    }

    [Fact]
    public void GetStringList_NullValue_ReturnsEmpty()
    {
        var dict = new Dictionary<string, object?> { ["tags"] = null };

        dict.GetStringList("tags").Should().BeEmpty();
    }

    [Fact]
    public void GetStringList_ListOfStrings_ReturnsList()
    {
        var dict = new Dictionary<string, object?>
        {
            ["tags"] = new List<object?> { "csharp", "testing" }
        };

        dict.GetStringList("tags").Should().BeEquivalentTo(new[] { "csharp", "testing" });
    }

    [Fact]
    public void GetStringList_ScalarString_WrapsInSingletonList()
    {
        var dict = new Dictionary<string, object?> { ["tags"] = "csharp" };

        dict.GetStringList("tags").Should().BeEquivalentTo(new[] { "csharp" });
    }

    [Fact]
    public void GetStringList_FiltersNullEntries()
    {
        var dict = new Dictionary<string, object?>
        {
            ["tags"] = new List<object?> { "a", null, "b" }
        };

        dict.GetStringList("tags").Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Fact]
    public void GetStringList_NonListNonString_ReturnsEmpty()
    {
        var dict = new Dictionary<string, object?> { ["tags"] = 42 };

        dict.GetStringList("tags").Should().BeEmpty();
    }

    [Fact]
    public void GetTargets_MissingKey_ReturnsEmpty()
    {
        var dict = new Dictionary<string, object?>();

        dict.GetTargets().Should().BeEmpty();
    }

    [Theory]
    [InlineData("copilot-cli", DeployTarget.CopilotCli)]
    [InlineData("copilotcli", DeployTarget.CopilotCli)]
    [InlineData("copilot", DeployTarget.CopilotCli)]
    [InlineData("claude-code", DeployTarget.ClaudeCode)]
    [InlineData("claudecode", DeployTarget.ClaudeCode)]
    [InlineData("claude", DeployTarget.ClaudeCode)]
    [InlineData("  CLAUDE-CODE  ", DeployTarget.ClaudeCode)]
    public void GetTargets_KnownTokens_ParseCorrectly(string token, DeployTarget expected)
    {
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { token }
        };

        dict.GetTargets().Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void GetTargets_UnknownToken_IsSilentlyDropped()
    {
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { "claude", "unknown-system" }
        };

        dict.GetTargets().Should().ContainSingle().Which.Should().Be(DeployTarget.ClaudeCode);
    }

    [Fact]
    public void GetTargets_AllUnknownTokens_ReturnsEmpty()
    {
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { "foo", "bar" }
        };

        dict.GetTargets().Should().BeEmpty();
    }
}
