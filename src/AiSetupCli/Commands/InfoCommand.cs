using System.ComponentModel;
using AiSetup.Lib.Discovery;
using AiSetup.Lib.Util;
using CreativeCoders.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Commands;

internal sealed class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly RepoRootResolver _rootResolver;
    private readonly IAnsiConsole _console;

    public InfoCommand(
        IAssetDiscovery discovery,
        RepoRootResolver rootResolver,
        IAnsiConsole console)
    {
        _discovery = Ensure.NotNull(discovery, nameof(discovery));
        _rootResolver = Ensure.NotNull(rootResolver, nameof(rootResolver));
        _console = Ensure.NotNull(console, nameof(console));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings, nameof(settings));

        var root = _rootResolver.Resolve(Environment.CurrentDirectory)
            ?? Environment.CurrentDirectory;

        var assets = _discovery.Discover(root);
        var match = assets.FirstOrDefault(a => a.Name.Equals(settings.Name, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            var suggestions = LevenshteinSuggester.Suggest(settings.Name, assets.Select(a => a.Name));
            _console.MarkupLineInterpolated($"[red]Asset '{settings.Name}' not found.[/]");

            if (suggestions.Count > 0)
            {
                _console.MarkupLineInterpolated($"[dim]Did you mean: {string.Join(", ", suggestions)}?[/]");
            }

            return 2;
        }

        var panel = new Panel(
            $"""
            [bold]Type[/]:        {match.Type}
            [bold]Description[/]: {Markup.Escape(match.Description)}
            [bold]Tags[/]:        {Markup.Escape(string.Join(", ", match.Tags))}
            [bold]Targets[/]:     {Markup.Escape(string.Join(", ", match.Targets))}
            [bold]ApplyTo[/]:     {Markup.Escape(match.ApplyTo ?? "(none)")}
            [bold]Path[/]:        {Markup.Escape(match.RelativePath)}
            """)
        {
            Header = new PanelHeader($" {match.Name} "),
            Border = BoxBorder.Rounded,
        };

        _console.Write(panel);

        var bodyPreview = match.Body.Length > 800 ? match.Body[..800] + "\n…" : match.Body;
        _console.MarkupLine("[bold]Body preview:[/]");
        _console.WriteLine(bodyPreview);

        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Name of the asset to inspect.")]
        [CommandArgument(0, "<name>")]
        public string Name { get; set; } = string.Empty;
    }
}
