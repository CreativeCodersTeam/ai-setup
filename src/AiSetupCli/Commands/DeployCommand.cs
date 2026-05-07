using System.ComponentModel;
using AiSetup.Cli.Infrastructure;
using AiSetup.Cli.Rendering;
using AiSetup.Lib.Deploy;
using AiSetup.Lib.Discovery;
using AiSetup.Lib.Exceptions;
using AiSetup.Lib.Models;
using CreativeCoders.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Commands;

internal sealed class DeployCommand : Command<DeployCommand.Settings>
{
    private readonly IDeployService _deployService;
    private readonly RepoRootResolver _rootResolver;
    private readonly PlanRenderer _renderer;
    private readonly IAnsiConsole _console;

    public DeployCommand(
        IDeployService deployService,
        RepoRootResolver rootResolver,
        PlanRenderer renderer,
        IAnsiConsole console)
    {
        _deployService = Ensure.NotNull(deployService, nameof(deployService));
        _rootResolver = Ensure.NotNull(rootResolver, nameof(rootResolver));
        _renderer = Ensure.NotNull(renderer, nameof(renderer));
        _console = Ensure.NotNull(console, nameof(console));
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings, nameof(settings));

        var sourceRoot = _rootResolver.Resolve(Environment.CurrentDirectory)
            ?? Environment.CurrentDirectory;

        var options = new DeployOptions(
            Target: settings.Target,
            Mode: settings.Mode,
            RepoPath: settings.RepoPath,
            Profile: settings.Profile,
            Agents: SplitList(settings.Agents),
            Instructions: SplitList(settings.Instructions),
            Skills: SplitList(settings.Skills),
            McpConfigs: SplitList(settings.McpConfigs),
            IncludeMcpConfigs: !settings.NoMcp,
            DryRun: settings.DryRun,
            Force: settings.Force);

        if (settings.Mode == DeployMode.Repo && string.IsNullOrWhiteSpace(settings.RepoPath))
        {
            _console.MarkupLine("[red]error:[/] --repo is required when --mode=repo.");

            return 1;
        }

        try
        {
            var result = _deployService.Deploy(sourceRoot, options);
            _renderer.Render(result.Plan, result.Warnings, settings.DryRun);

            if (!result.Applied && !settings.DryRun)
            {
                _console.MarkupLine("[dim]Nothing applied.[/]");
            }

            return 0;
        }
        catch (MissingAssetException ex)
        {
            _console.MarkupLineInterpolated($"[red]error:[/] {ex.Message}");

            return 2;
        }
        catch (AiSetupException ex)
        {
            _console.MarkupLineInterpolated($"[red]error:[/] {ex.Message}");

            return 1;
        }
    }

    private static IReadOnlyList<string> SplitList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Target AI system (copilot-cli or claude-code).")]
        [CommandOption("-t|--target")]
        [TypeConverter(typeof(DeployTargetConverter))]
        public DeployTarget Target { get; set; } = DeployTarget.ClaudeCode;

        [Description("Deploy mode (repo or local).")]
        [CommandOption("-m|--mode")]
        [TypeConverter(typeof(DeployModeConverter))]
        public DeployMode Mode { get; set; } = DeployMode.Repo;

        [Description("Target repository path (required with --mode=repo).")]
        [CommandOption("-r|--repo <PATH>")]
        public string? RepoPath { get; set; }

        [Description("Profile name (resolved against profiles/<name>.yaml).")]
        [CommandOption("-p|--profile <NAME>")]
        public string? Profile { get; set; }

        [Description("Comma-separated list of agent names.")]
        [CommandOption("--agents <LIST>")]
        public string? Agents { get; set; }

        [Description("Comma-separated list of instruction names.")]
        [CommandOption("--instructions <LIST>")]
        public string? Instructions { get; set; }

        [Description("Comma-separated list of skill names.")]
        [CommandOption("--skills <LIST>")]
        public string? Skills { get; set; }

        [Description("Comma-separated list of MCP config names.")]
        [CommandOption("--mcp-configs <LIST>")]
        public string? McpConfigs { get; set; }

        [Description("Skip MCP configs even if referenced by a profile.")]
        [CommandOption("--no-mcp")]
        public bool NoMcp { get; set; }

        [Description("Show planned actions without writing.")]
        [CommandOption("--dry-run")]
        public bool DryRun { get; set; }

        [Description("Overwrite existing files without prompt.")]
        [CommandOption("-f|--force")]
        public bool Force { get; set; }
    }
}
