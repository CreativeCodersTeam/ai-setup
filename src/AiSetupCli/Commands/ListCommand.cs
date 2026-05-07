using System.ComponentModel;
using AiSetup.Cli.Infrastructure;
using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using CreativeCoders.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Commands;

/// <summary>
/// <c>ai-setup list</c> command. Prints assets or profiles in a table.
/// </summary>
public sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;

    /// <summary>Initializes a new instance.</summary>
    public ListCommand(IFileSystem fileSystem, IAnsiConsole console)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
        _console = Ensure.NotNull(console);
    }

    /// <inheritdoc />
    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings);

        var sourceRepo = ResolveSourceRepo(settings.SourceRepo);

        if (string.Equals(settings.Kind, "profiles", StringComparison.OrdinalIgnoreCase))
        {
            RenderProfiles(sourceRepo);
            return 0;
        }

        RenderAssets(sourceRepo, settings);
        return 0;
    }

    private void RenderProfiles(string sourceRepo)
    {
        var repo = new YamlProfileRepository(_fileSystem, sourceRepo);
        var table = new Table().AddColumns("Name", "Description", "Skills", "Agents", "Instructions", "MCP");

        foreach (var profile in repo.All().OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            table.AddRow(
                profile.Name,
                profile.Description ?? string.Empty,
                profile.Skills.Count.ToString(),
                profile.Agents.Count.ToString(),
                profile.Instructions.Count.ToString(),
                profile.McpConfigs.Count.ToString());
        }

        _console.Write(table);
    }

    private void RenderAssets(string sourceRepo, Settings settings)
    {
        var repo = new FileSystemAssetRepository(_fileSystem, sourceRepo);
        var typeFilter = ParseAssetType(settings.Kind);

        var assets = repo.All(typeFilter)
            .Where(a => string.IsNullOrWhiteSpace(settings.Tag) || a.Tags.Contains(settings.Tag))
            .Where(a => settings.Target is null || a.Targets.Count == 0 || a.Targets.Contains(settings.Target.Value))
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Id, StringComparer.Ordinal)
            .ToArray();

        var table = new Table().AddColumns("Type", "ID", "Name", "Tags", "Targets");

        foreach (var asset in assets)
        {
            table.AddRow(
                asset.Type.ToString(),
                asset.Id,
                asset.Name,
                string.Join(", ", asset.Tags),
                asset.Targets.Count == 0 ? "all" : string.Join(", ", asset.Targets));
        }

        _console.Write(table);
    }

    private static AssetType? ParseAssetType(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return null;
        }

        return kind.ToLowerInvariant() switch
        {
            "agents" or "agent" => AssetType.Agent,
            "skills" or "skill" => AssetType.Skill,
            "instructions" or "instruction" => AssetType.Instruction,
            "mcp-configs" or "mcp" or "mcp-config" => AssetType.McpConfig,
            _ => throw new AiSetupException($"Unknown asset kind '{kind}'.")
        };
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

    /// <summary>Settings for <see cref="ListCommand"/>.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Asset kind or "profiles".</summary>
        [CommandArgument(0, "[KIND]")]
        [Description("Asset kind: agents, skills, instructions, mcp-configs, or profiles.")]
        public string? Kind { get; init; }

        /// <summary>Filter by tag.</summary>
        [CommandOption("--tag <TAG>")]
        public string? Tag { get; init; }

        /// <summary>Filter by target system.</summary>
        [CommandOption("--target <TARGET>")]
        [TypeConverter(typeof(DeployTargetConverter))]
        public DeployTarget? Target { get; init; }

        /// <summary>Source repo override.</summary>
        [CommandOption("-s|--source <PATH>")]
        public string? SourceRepo { get; init; }
    }
}
