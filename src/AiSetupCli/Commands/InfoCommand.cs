using AiSetupLib.Discovery;
using AiSetupLib.Deploy;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly IAnsiConsole _console;
    private readonly string _repoRoot;

    public InfoCommand(IAssetDiscovery discovery, IAnsiConsole console, RepoRoot repoRoot)
    {
        _discovery = discovery;
        _console = console;
        _repoRoot = repoRoot.Path;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<NAME>")]
        public string Name { get; init; } = "";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var assets = _discovery.Discover(_repoRoot);
        var match = assets.FirstOrDefault(a => a.Name == settings.Name);
        if (match is null)
        {
            var hints = Suggestions.Closest(settings.Name, assets.Select(a => a.Name));
            _console.MarkupLine($"[red]Asset '{Markup.Escape(settings.Name)}' not found.[/]");
            if (hints.Count > 0)
                _console.MarkupLine($"[yellow]Did you mean: {string.Join(", ", hints.Select(Markup.Escape))}?[/]");
            return 1;
        }

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]Name[/]", Markup.Escape(match.Name));
        grid.AddRow("[bold]Type[/]", match.Type.ToString());
        grid.AddRow("[bold]Description[/]", Markup.Escape(match.Description));
        grid.AddRow("[bold]Tags[/]", Markup.Escape(string.Join(", ", match.Tags)));
        grid.AddRow("[bold]Targets[/]", Markup.Escape(string.Join(", ", match.Targets)));
        grid.AddRow("[bold]Source[/]", Markup.Escape(match.SourcePath));
        _console.Write(grid);
        return 0;
    }
}
