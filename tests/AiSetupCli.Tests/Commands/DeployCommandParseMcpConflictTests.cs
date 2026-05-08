using AiSetup.Cli.Commands;
using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Cli.Tests.Commands;

public sealed class DeployCommandParseMcpConflictTests
{
    [Fact]
    public void ParseMcpConflict_NullWithoutForce_ReturnsFail()
    {
        DeployCommand.ParseMcpConflict(null, force: false)
            .Should().Be(McpConflictResolution.Fail);
    }

    [Fact]
    public void ParseMcpConflict_NullWithForce_ReturnsOverwrite()
    {
        DeployCommand.ParseMcpConflict(null, force: true)
            .Should().Be(McpConflictResolution.Overwrite);
    }

    [Fact]
    public void ParseMcpConflict_WhitespaceWithForce_ReturnsOverwrite()
    {
        DeployCommand.ParseMcpConflict("   ", force: true)
            .Should().Be(McpConflictResolution.Overwrite);
    }

    [Theory]
    [InlineData("fail", McpConflictResolution.Fail)]
    [InlineData("overwrite", McpConflictResolution.Overwrite)]
    [InlineData("skip", McpConflictResolution.Skip)]
    [InlineData("SKIP", McpConflictResolution.Skip)]
    [InlineData(" Skip ", McpConflictResolution.Skip)]
    public void ParseMcpConflict_KnownValue_ReturnsMatchingEnum(string value, McpConflictResolution expected)
    {
        DeployCommand.ParseMcpConflict(value, force: false)
            .Should().Be(expected);
    }

    [Fact]
    public void ParseMcpConflict_SkipWithForce_ExplicitValueWins()
    {
        DeployCommand.ParseMcpConflict("skip", force: true)
            .Should().Be(McpConflictResolution.Skip);
    }

    [Fact]
    public void ParseMcpConflict_InvalidValue_ThrowsAiSetupException()
    {
        Action act = () => DeployCommand.ParseMcpConflict("bogus", force: false);

        act.Should().Throw<AiSetupException>()
            .WithMessage("*Invalid*--mcp-on-conflict*bogus*");
    }
}
