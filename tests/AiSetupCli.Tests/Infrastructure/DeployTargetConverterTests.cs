using System.Globalization;
using AiSetup.Cli.Infrastructure;
using AiSetup.Models;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class DeployTargetConverterTests
{
    [Theory]
    [InlineData("copilot-cli", DeployTarget.CopilotCli)]
    [InlineData("copilotcli", DeployTarget.CopilotCli)]
    [InlineData("copilot", DeployTarget.CopilotCli)]
    [InlineData("claude-code", DeployTarget.ClaudeCode)]
    [InlineData("claudecode", DeployTarget.ClaudeCode)]
    [InlineData("claude", DeployTarget.ClaudeCode)]
    [InlineData("CLAUDE-CODE", DeployTarget.ClaudeCode)]
    [InlineData("  copilot  ", DeployTarget.CopilotCli)]
    public void ConvertFrom_WithKnownTokens_ReturnsExpectedTarget(string raw, DeployTarget expected)
    {
        // Arrange
        var sut = new DeployTargetConverter();

        // Act
        var result = sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("")]
    public void ConvertFrom_WithUnknownString_ThrowsFormatException(string raw)
    {
        // Arrange
        var sut = new DeployTargetConverter();

        // Act
        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        // Assert
        act.Should().Throw<FormatException>().WithMessage("*Unknown deploy target*");
    }

    [Fact]
    public void CanConvertFrom_WithStringType_ReturnsTrue()
    {
        // Arrange
        var sut = new DeployTargetConverter();

        // Act
        var result = sut.CanConvertFrom(null, typeof(string));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ConvertFrom_WithNonStringValue_ThrowsNotSupportedException()
    {
        // Arrange
        var sut = new DeployTargetConverter();

        // Act
        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, 42);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
