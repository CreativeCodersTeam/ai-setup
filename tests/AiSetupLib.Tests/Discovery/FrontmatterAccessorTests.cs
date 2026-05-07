using AiSetup.Discovery;
using AiSetup.Models;

namespace AiSetup.Tests.Discovery;

public sealed class FrontmatterAccessorTests
{
    [Fact]
    public void GetString_WithMissingKey_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, object?>();

        // Act
        var result = dict.GetString("name");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetString_WithStringValue_ReturnsValue()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["name"] = "foo" };

        // Act
        var result = dict.GetString("name");

        // Assert
        result.Should().Be("foo");
    }

    [Fact]
    public void GetString_WithNullValue_ReturnsNull()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["name"] = null };

        // Act
        var result = dict.GetString("name");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetString_WithNonStringValue_UsesToString()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["count"] = 42 };

        // Act
        var result = dict.GetString("count");

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void GetStringList_WithMissingKey_ReturnsEmpty()
    {
        // Arrange
        var dict = new Dictionary<string, object?>();

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStringList_WithNullValue_ReturnsEmpty()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["tags"] = null };

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStringList_WithListOfStrings_ReturnsList()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["tags"] = new List<object?> { "csharp", "testing" }
        };

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEquivalentTo(new[] { "csharp", "testing" });
    }

    [Fact]
    public void GetStringList_WithScalarString_WrapsInSingletonList()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["tags"] = "csharp" };

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEquivalentTo(new[] { "csharp" });
    }

    [Fact]
    public void GetStringList_WithNullEntriesInList_FiltersOutNulls()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["tags"] = new List<object?> { "a", null, "b" }
        };

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [Fact]
    public void GetStringList_WithNonListNonString_ReturnsEmpty()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { ["tags"] = 42 };

        // Act
        var result = dict.GetStringList("tags");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTargets_WithMissingKey_ReturnsEmpty()
    {
        // Arrange
        var dict = new Dictionary<string, object?>();

        // Act
        var result = dict.GetTargets();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("copilot-cli", DeployTarget.CopilotCli)]
    [InlineData("copilotcli", DeployTarget.CopilotCli)]
    [InlineData("copilot", DeployTarget.CopilotCli)]
    [InlineData("claude-code", DeployTarget.ClaudeCode)]
    [InlineData("claudecode", DeployTarget.ClaudeCode)]
    [InlineData("claude", DeployTarget.ClaudeCode)]
    [InlineData("  CLAUDE-CODE  ", DeployTarget.ClaudeCode)]
    public void GetTargets_WithKnownTokens_ParsesToCorrectTarget(string token, DeployTarget expected)
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { token }
        };

        // Act
        var result = dict.GetTargets();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void GetTargets_WithUnknownToken_SilentlyDropsUnknownEntry()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { "claude", "unknown-system" }
        };

        // Act
        var result = dict.GetTargets();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(DeployTarget.ClaudeCode);
    }

    [Fact]
    public void GetTargets_WithAllUnknownTokens_ReturnsEmpty()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["targets"] = new List<object?> { "foo", "bar" }
        };

        // Act
        var result = dict.GetTargets();

        // Assert
        result.Should().BeEmpty();
    }
}
