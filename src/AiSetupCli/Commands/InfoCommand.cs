using System.ComponentModel;
using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;
using CreativeCoders.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Commands;

/// <summary>
/// <c>ai-setup info</c> command. Prints metadata and content preview for a single asset.
/// </summary>
public sealed class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;

    /// <summary>Initializes a new instance.</summary>
    public InfoCommand(IFileSystem fileSystem, IAnsiConsole console)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
        _console = Ensure.NotNull(console);
    }

    /// <inheritdoc />
    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings);

        if (string.IsNullOrWhiteSpace(settings.AssetId))
        {
            _console.MarkupLine("[red]Error:[/] asset ID is required.");
            return 2;
        }

        var sourceRepo = ResolveSourceRepo(settings.SourceRepo);
        var repo = new FileSystemAssetRepository(_fileSystem, sourceRepo);

        var matches = repo.All()
            .Where(a => string.Equals(a.Id, settings.AssetId, StringComparison.Ordinal))
            .ToArray();

        if (matches.Length == 0)
        {
            _console.MarkupLineInterpolated($"[red]Asset '{settings.AssetId}' not found.[/]");
            return 1;
        }

        foreach (var asset in matches)
        {
            RenderAsset(asset);
        }

        return 0;
    }

    private void RenderAsset(AssetDefinition asset)
    {
        _console.Write(new Rule($"[bold]{asset.Type}: {asset.Id}[/]") { Justification = Justify.Left });

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("Name", asset.Name);
        grid.AddRow("Description", asset.Description);
        grid.AddRow("Tags", string.Join(", ", asset.Tags));
        grid.AddRow("Targets", asset.Targets.Count == 0 ? "all" : string.Join(", ", asset.Targets));

        if (!string.IsNullOrWhiteSpace(asset.ApplyTo))
        {
            grid.AddRow("ApplyTo", asset.ApplyTo);
        }

        grid.AddRow("Source", asset.SourcePath);

        if (asset is SkillAsset skill)
        {
            grid.AddRow("Files", string.Join("\n", skill.Files));
        }

        _console.Write(grid);
    }

    private string ResolveSourceRepo(string? candidate)
    {
        var path = string.IsNullOrWhiteSpace(candidate) ? Environment.CurrentDirectory : candidate;

        if (!_fileSystem.DirectoryExists(path))
        {
            throw new AiSetupException($"Source repository path '{path}' does not exist.");
        }

        return Path.GetFullPath(path);
    }

    /// <summary>Settings for <see cref="InfoCommand"/>.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Asset ID.</summary>
        [CommandArgument(0, "<ASSET_ID>")]
        [Description("Asset ID, e.g. csharp/dotnet-tester")]
        public string? AssetId { get; init; }

        /// <summary>Source repo override.</summary>
        [CommandOption("-s|--source <PATH>")]
        public string? SourceRepo { get; init; }
    }
}
