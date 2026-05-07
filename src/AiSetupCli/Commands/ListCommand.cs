using System.ComponentModel;
using AiSetup.Cli.Infrastructure;
using AiSetup.Lib.Discovery;
using AiSetup.Lib.Models;
using AiSetup.Lib.Profiles;
using CreativeCoders.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Commands;

internal sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly IProfileResolver _profileResolver;
    private readonly RepoRootResolver _rootResolver;
    private readonly IAnsiConsole _console;

    public ListCommand(
        IAssetDiscovery discovery,
        IProfileResolver profileResolver,
        RepoRootResolver rootResolver,
        IAnsiConsole console)
    {
        _discovery = Ensure.NotNull(discovery, nameof(discovery));
        _profileResolver = Ensure.NotNull(profileResolver, nameof(profileResolver));
        _rootResolver = Ensure.NotNull(rootResolver, nameof(rootResolver));
        _console = Ensure.NotNull(console, nameof(console));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings, nameof(settings));

        var root = _rootResolver.Resolve(Environment.CurrentDirectory)
            ?? Environment.CurrentDirectory;

        var kind = (settings.Kind ?? "all").ToLowerInvariant();

        if (kind is "profiles" or "profile")
        {
            RenderProfiles(root);

            return 0;
        }

        var assetType = kind switch
        {
            "agents" or "agent" => (AssetType?)AssetType.Agent,
            "skills" or "skill" => AssetType.Skill,
            "instructions" or "instruction" => AssetType.Instruction,
            "mcp-configs" or "mcp-config" or "mcpconfigs" or "mcpconfig" => AssetType.McpConfig,
            "all" => null,
            _ => throw new ArgumentException($"Unknown list kind '{settings.Kind}'.", nameof(settings)),
        };

        var assets = _discovery.Discover(root)
            .Where(a => assetType is null || a.Type == assetType)
            .Where(a => string.IsNullOrEmpty(settings.Tag) || a.Tags.Contains(settings.Tag, StringComparer.OrdinalIgnoreCase))
            .Where(a => !settings.TargetFilter.HasValue || a.Targets.Count == 0 || a.Targets.Contains(settings.TargetFilter.Value))
            .OrderBy(a => a.Type)
            .ThenBy(a => a.Name, StringComparer.Ordinal)
            .ToArray();

        var table = new Table().AddColumns("Type", "Name", "Description", "Tags");
        table.Border = TableBorder.Rounded;

        foreach (var asset in assets)
        {
            table.AddRow(
                Markup.Escape(asset.Type.ToString()),
                Markup.Escape(asset.Name),
                Markup.Escape(asset.Description),
                Markup.Escape(string.Join(", ", asset.Tags)));
        }

        _console.Write(table);
        _console.MarkupLineInterpolated($"[dim]{assets.Length} asset(s)[/]");

        return 0;
    }

    private void RenderProfiles(string root)
    {
        var table = new Table().AddColumns("Name", "Description", "Agents", "Instructions", "Skills", "MCP");
        table.Border = TableBorder.Rounded;

        foreach (var profile in _profileResolver.ListProfiles(root).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            table.AddRow(
                Markup.Escape(profile.Name),
                Markup.Escape(profile.Description ?? string.Empty),
                profile.Agents.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                profile.Instructions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                profile.Skills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                profile.McpConfigs.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        _console.Write(table);
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Asset kind to list: agents, skills, instructions, mcp-configs, profiles, or all.")]
        [CommandArgument(0, "[kind]")]
        public string? Kind { get; set; }

        [Description("Filter by tag.")]
        [CommandOption("--tag <TAG>")]
        public string? Tag { get; set; }

        [Description("Filter by supported target.")]
        [CommandOption("--target <TARGET>")]
        [TypeConverter(typeof(DeployTargetConverter))]
        public DeployTarget? TargetFilter { get; set; }
    }
}
