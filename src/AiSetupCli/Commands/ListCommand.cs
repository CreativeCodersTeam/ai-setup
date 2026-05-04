using System.ComponentModel;
using AiSetupLib.Discovery;
using AiSetupLib.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly IAnsiConsole _console;
    private readonly string _repoRoot;

    public ListCommand(IAssetDiscovery discovery, IAnsiConsole console, RepoRoot repoRoot)
    {
        _discovery = discovery;
        _console = console;
        _repoRoot = repoRoot.Path;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[KIND]")]
        [Description("agents | skills | instructions | mcp-configs | profiles")]
        public string? Kind { get; init; }

        [CommandOption("--tag <TAG>")]
        public string? Tag { get; init; }

        [CommandOption("--target <TARGET>")]
        public string? Target { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var assets = _discovery.Discover(_repoRoot);

        var filtered = assets.AsEnumerable();
        if (settings.Kind is { } k && KindToType(k) is { } type)
            filtered = filtered.Where(a => a.Type == type);
        if (!string.IsNullOrEmpty(settings.Tag))
            filtered = filtered.Where(a => a.Tags.Contains(settings.Tag));
        if (settings.Target is { } t && TargetMap.TryGetValue(t, out var tgt))
            filtered = filtered.Where(a => a.Targets.Count == 0 || a.Targets.Contains(tgt));

        var table = new Table()
            .AddColumn("Type").AddColumn("Name").AddColumn("Tags").AddColumn("Description");
        foreach (var asset in filtered.OrderBy(a => a.Type).ThenBy(a => a.Name))
        {
            table.AddRow(
                asset.Type.ToString(),
                Markup.Escape(asset.Name),
                Markup.Escape(string.Join(",", asset.Tags)),
                Markup.Escape(asset.Description));
        }
        _console.Write(table);
        return 0;
    }

    private static AssetType? KindToType(string kind) => kind switch
    {
        "agents" => AssetType.Agent,
        "skills" => AssetType.Skill,
        "instructions" => AssetType.Instruction,
        "mcp-configs" => AssetType.McpConfig,
        _ => null,
    };

    private static readonly Dictionary<string, DeployTarget> TargetMap = new()
    {
        ["copilot-cli"] = DeployTarget.CopilotCli,
        ["claude-code"] = DeployTarget.ClaudeCode,
    };
}

internal sealed record RepoRoot(string Path);
