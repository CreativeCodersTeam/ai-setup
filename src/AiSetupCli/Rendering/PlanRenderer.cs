using AiSetup.Models;
using CreativeCoders.Core;
using Spectre.Console;

namespace AiSetup.Cli.Rendering;

/// <summary>
/// Pretty-prints a <see cref="DeployReport"/> using colors:
/// green = create, yellow = overwrite, grey = skip, red = error.
/// </summary>
public sealed class PlanRenderer
{
    private readonly IAnsiConsole _console;

    /// <summary>Initializes a new instance.</summary>
    public PlanRenderer(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console);
    }

    /// <summary>Renders the report.</summary>
    public void Render(DeployReport report)
    {
        Ensure.NotNull(report);

        var header = report.DryRun
            ? $"[bold yellow]Dry-run: {report.Plan.Actions.Count} action(s) planned for {report.Plan.Target} ({report.Plan.Mode})[/]"
            : $"[bold]Deploy: {report.Plan.Target} ({report.Plan.Mode})[/]";

        _console.Write(new Rule(header) { Justification = Justify.Left });

        foreach (var action in report.Plan.Actions)
        {
            _console.MarkupLine($"  {Marker(action.Status)} {Markup.Escape(action.Description)}");
            _console.MarkupLine($"      [grey]{Markup.Escape(action.TargetPath)}[/]");
        }

        if (!report.DryRun)
        {
            _console.MarkupLineInterpolated(
                $"[green]Executed:[/] {report.Executed.Count}, [yellow]skipped:[/] {report.Skipped.Count}, [red]errors:[/] {report.Errors.Count}");

            foreach (var (action, error) in report.Errors)
            {
                _console.MarkupLineInterpolated($"[red]  ! {action.TargetPath}: {error}[/]");
            }
        }
    }

    private static string Marker(DeployActionStatus status)
    {
        return status switch
        {
            DeployActionStatus.Create => "[green]+ CREATE   [/]",
            DeployActionStatus.Overwrite => "[yellow]~ OVERWRITE[/]",
            DeployActionStatus.Skip => "[grey]· SKIP     [/]",
            DeployActionStatus.Error => "[red]! ERROR    [/]",
            _ => status.ToString()
        };
    }
}
