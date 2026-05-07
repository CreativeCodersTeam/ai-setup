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
    public void ConvertFrom_KnownTokens_ReturnsExpectedTarget(string raw, DeployTarget expected)
    {
        var sut = new DeployTargetConverter();

        var result = sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("")]
    public void ConvertFrom_UnknownString_Throws(string raw)
    {
        var sut = new DeployTargetConverter();

        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, raw);

        act.Should().Throw<FormatException>().WithMessage("*Unknown deploy target*");
    }

    [Fact]
    public void CanConvertFrom_String_ReturnsTrue()
    {
        var sut = new DeployTargetConverter();

        sut.CanConvertFrom(null, typeof(string)).Should().BeTrue();
    }

    [Fact]
    public void ConvertFrom_NonStringValue_DelegatesToBase()
    {
        var sut = new DeployTargetConverter();

        Action act = () => sut.ConvertFrom(null, CultureInfo.InvariantCulture, 42);

        act.Should().Throw<NotSupportedException>();
    }
}
