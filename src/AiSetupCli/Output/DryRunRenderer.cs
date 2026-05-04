using AiSetupLib.Models;
using Spectre.Console;

namespace AiSetupCli.Output;

internal static class DryRunRenderer
{
    public static void Render(IAnsiConsole console, DeployPlan plan)
    {
        var table = new Table().AddColumn("Action").AddColumn("Target").AddColumn("Sources");

        foreach (var action in plan.Actions)
        {
            var color = action.Kind switch
            {
                DeployActionKind.Create => "green",
                DeployActionKind.Overwrite => "yellow",
                DeployActionKind.Skip => "grey",
                DeployActionKind.Error => "red",
                _ => "white",
            };
            table.AddRow(
                $"[{color}]{action.Kind}[/]",
                Markup.Escape(action.TargetPath),
                Markup.Escape(string.Join(", ", action.SourceAssets)));
        }

        console.Write(table);
    }
}
