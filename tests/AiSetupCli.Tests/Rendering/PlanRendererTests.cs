using AiSetup.Cli.Rendering;
using AiSetup.Models;
using Spectre.Console.Testing;

namespace AiSetup.Cli.Tests.Rendering;

public sealed class PlanRendererTests
{
    [Fact]
    public void Render_WithDryRunReport_PrintsHeaderAndPlannedActions()
    {
        // Arrange
        var console = new TestConsole();
        var sut = new PlanRenderer(console);

        var plan = new DeployPlan(DeployTarget.ClaudeCode,
            [new WriteFileAction("/x.md", "x", DeployActionStatus.Create, "Write x")]);
        var report = new DeployReport(plan, [], [], [], DryRun: true);

        // Act
        sut.Render(report);

        // Assert
        console.Output.Should().Contain("Dry-run");
        console.Output.Should().Contain("CREATE");
        console.Output.Should().Contain("Write x");
        console.Output.Should().Contain("/x.md");
    }

    [Fact]
    public void Render_WithExecutedReport_PrintsExecutedSkippedAndErrorCounts()
    {
        // Arrange
        var console = new TestConsole();
        var sut = new PlanRenderer(console);

        var plan = new DeployPlan(DeployTarget.ClaudeCode,
            [new WriteFileAction("/a", "x", DeployActionStatus.Create, "a")]);
        var report = new DeployReport(plan,
            Executed: [new WriteFileAction("/a", "x", DeployActionStatus.Create, "a")],
            Skipped: [],
            Errors: [],
            DryRun: false);

        // Act
        sut.Render(report);

        // Assert
        console.Output.Should().Contain("Executed:");
        console.Output.Should().Contain("skipped:");
        console.Output.Should().Contain("errors:");
    }

    [Fact]
    public void Render_WithErrorsInReport_PrintsErrorDetails()
    {
        // Arrange
        var console = new TestConsole();
        var sut = new PlanRenderer(console);

        var failing = new WriteFileAction("/boom", "x", DeployActionStatus.Error, "fail");
        var plan = new DeployPlan(DeployTarget.ClaudeCode, [failing]);
        var report = new DeployReport(plan,
            Executed: [],
            Skipped: [],
            Errors: [(failing, "io blew up")],
            DryRun: false);

        // Act
        sut.Render(report);

        // Assert
        console.Output.Should().Contain("/boom");
        console.Output.Should().Contain("io blew up");
    }

    [Theory]
    [InlineData(DeployActionStatus.Create, "CREATE")]
    [InlineData(DeployActionStatus.Overwrite, "OVERWRITE")]
    [InlineData(DeployActionStatus.Skip, "SKIP")]
    [InlineData(DeployActionStatus.Error, "ERROR")]
    public void Render_WithStatus_RendersExpectedStatusMarker(DeployActionStatus status, string expectedToken)
    {
        // Arrange
        var console = new TestConsole();
        var sut = new PlanRenderer(console);

        var action = new WriteFileAction("/p", "x", status, "desc");
        var plan = new DeployPlan(DeployTarget.ClaudeCode, [action]);
        var report = new DeployReport(plan, [], [], [], DryRun: true);

        // Act
        sut.Render(report);

        // Assert
        console.Output.Should().Contain(expectedToken);
    }

    [Fact]
    public void Render_WithNullReport_ThrowsArgumentNullException()
    {
        // Arrange
        var console = new TestConsole();
        var sut = new PlanRenderer(console);

        // Act
        Action act = () => sut.Render(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
