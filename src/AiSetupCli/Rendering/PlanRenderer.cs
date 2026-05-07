using AiSetup.Lib.Models;
using CreativeCoders.Core;
using Spectre.Console;

namespace AiSetup.Cli.Rendering;

internal sealed class PlanRenderer
{
    private readonly IAnsiConsole _console;

    public PlanRenderer(IAnsiConsole console)
    {
        _console = Ensure.NotNull(console, nameof(console));
    }

    public void Render(DeployPlan plan, IReadOnlyList<string> warnings, bool dryRun)
    {
        if (warnings.Count > 0)
        {
            foreach (var warning in warnings)
            {
                _console.MarkupLineInterpolated($"[yellow]warning:[/] {warning}");
            }
        }

        var header = dryRun ? "[bold yellow]Dry-Run plan[/]" : "[bold green]Deploy plan[/]";

        _console.MarkupLineInterpolated($"{header} — target [cyan]{plan.Target}[/], mode [cyan]{plan.Mode}[/]");

        if (plan.Actions.Count == 0)
        {
            _console.MarkupLine("[dim]No actions planned.[/]");

            return;
        }

        var table = new Table().AddColumns("Kind", "Target", "Source");
        table.Border = TableBorder.Rounded;

        foreach (var action in plan.Actions)
        {
            var kindMarkup = action.Kind switch
            {
                DeployActionKind.Create => "[green]create[/]",
                DeployActionKind.Overwrite => "[yellow]overwrite[/]",
                DeployActionKind.Skip => "[dim]skip[/]",
                DeployActionKind.Error => "[red]error[/]",
                _ => action.Kind.ToString().ToLowerInvariant(),
            };

            var source = action.SourcePath ?? (action.IsDirectoryCopy ? "[dim]<copy>[/]" : "[dim]<aggregated>[/]");
            table.AddRow(kindMarkup, Markup.Escape(action.TargetPath), source.StartsWith('[') ? source : Markup.Escape(source));
        }

        _console.Write(table);
    }
}
