using System.ComponentModel;
using AiSetup.Cli.Infrastructure;
using AiSetup.Cli.Rendering;
using AiSetup.Deploy;
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
/// <c>ai-setup deploy</c> command. Resolves a profile and CLI overrides into a deploy plan
/// and executes it against the chosen target system.
/// </summary>
public sealed class DeployCommand : Command<DeployCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;
    private readonly PlanRenderer _renderer;

    /// <summary>Initializes a new instance.</summary>
    public DeployCommand(IFileSystem fileSystem, IAnsiConsole console, PlanRenderer renderer)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
        _console = Ensure.NotNull(console);
        _renderer = Ensure.NotNull(renderer);
    }

    /// <inheritdoc />
    public override int Execute(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings);

        var sourceRepo = ResolveSourceRepo(settings.SourceRepo);
        var assets = new FileSystemAssetRepository(_fileSystem, sourceRepo);
        var profiles = new YamlProfileRepository(_fileSystem, sourceRepo);
        var resolver = new ProfileResolver(assets, profiles);
        var registry = new Targets.TargetRegistry(new Targets.IDeployTarget[]
        {
            new Targets.CopilotCliTarget(_fileSystem, new PathProvider(),
                new Aggregation.MarkdownAggregator(), new Aggregation.McpConfigMerger()),
            new Targets.ClaudeCodeTarget(_fileSystem, new PathProvider(),
                new Aggregation.MarkdownAggregator(), new Aggregation.McpConfigMerger())
        });
        var service = new DeployService(resolver, registry, _fileSystem);

        var options = new DeployOptions
        {
            Target = settings.Target,
            Mode = settings.Mode,
            SourceRepoPath = sourceRepo,
            DestinationRepoPath = settings.DestinationRepo,
            ProfileName = settings.Profile,
            Agents = CliOptionParser.Normalize(settings.Agents),
            Instructions = CliOptionParser.Normalize(settings.Instructions),
            Skills = CliOptionParser.Normalize(settings.Skills),
            McpConfigs = CliOptionParser.Normalize(settings.McpConfigs),
            DryRun = settings.DryRun,
            Force = settings.Force,
            McpConflict = ParseMcpConflict(settings.McpOnConflict, settings.Force)
        };

        try
        {
            var report = service.Deploy(options);
            _renderer.Render(report);

            if (assets.Warnings.Count > 0)
            {
                _console.MarkupLine($"[yellow]Warnings during discovery: {assets.Warnings.Count}[/]");
                foreach (var warning in assets.Warnings)
                {
                    _console.MarkupLineInterpolated($"[yellow]  - {warning}[/]");
                }
            }

            return report.Errors.Count == 0 ? 0 : 1;
        }
        catch (AiSetupException ex)
        {
            _console.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            return 2;
        }
    }

    /// <inheritdoc />
    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        Ensure.NotNull(settings);

        if (settings.Mode == DeployMode.Repo && string.IsNullOrWhiteSpace(settings.DestinationRepo))
        {
            return ValidationResult.Error("--repo is required when --mode is 'repo'.");
        }

        return base.Validate(context, settings);
    }

    internal static McpConflictResolution ParseMcpConflict(string? value, bool force)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return force ? McpConflictResolution.Overwrite : McpConflictResolution.Fail;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "fail" => McpConflictResolution.Fail,
            "overwrite" => McpConflictResolution.Overwrite,
            "skip" => McpConflictResolution.Skip,
            _ => throw new AiSetupException(
                $"Invalid --mcp-on-conflict value '{value}'. Allowed: fail, overwrite, skip.")
        };
    }

    private string ResolveSourceRepo(string? candidate)
    {
        var path = string.IsNullOrWhiteSpace(candidate)
            ? Environment.CurrentDirectory
            : candidate;

        if (!_fileSystem.DirectoryExists(path))
        {
            throw new AiSetupException($"Source repository path '{path}' does not exist.");
        }

        return Path.GetFullPath(path);
    }

    /// <summary>Settings for <see cref="DeployCommand"/>.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Target system to deploy to.</summary>
        [CommandOption("-t|--target <TARGET>")]
        [Description("Target system: copilot-cli or claude-code.")]
        [TypeConverter(typeof(DeployTargetConverter))]
        public DeployTarget Target { get; init; }

        /// <summary>Where to deploy (repo or local).</summary>
        [CommandOption("-m|--mode <MODE>")]
        [Description("Deploy mode: repo or local.")]
        [TypeConverter(typeof(DeployModeConverter))]
        public DeployMode Mode { get; init; } = DeployMode.Repo;

        /// <summary>Path of the destination repository (when --mode=repo).</summary>
        [CommandOption("-r|--repo <PATH>")]
        [Description("Destination repository path (required when --mode=repo).")]
        public string? DestinationRepo { get; init; }

        /// <summary>Override the source ai-setup repo path.</summary>
        [CommandOption("-s|--source <PATH>")]
        [Description("Source ai-setup repository path. Defaults to the current directory.")]
        public string? SourceRepo { get; init; }

        /// <summary>Profile name.</summary>
        [CommandOption("-p|--profile <NAME>")]
        [Description("Profile name (file stem under profiles/).")]
        public string? Profile { get; init; }

        /// <summary>Additional agent IDs.</summary>
        [CommandOption("--agents <LIST>")]
        [Description("Agent IDs to include (repeat or comma-separated).")]
        public string[]? Agents { get; init; }

        /// <summary>Additional instruction IDs.</summary>
        [CommandOption("--instructions <LIST>")]
        [Description("Instruction IDs to include (repeat or comma-separated).")]
        public string[]? Instructions { get; init; }

        /// <summary>Additional skill IDs.</summary>
        [CommandOption("--skills <LIST>")]
        [Description("Skill IDs to include (repeat or comma-separated).")]
        public string[]? Skills { get; init; }

        /// <summary>Additional MCP config IDs.</summary>
        [CommandOption("--mcp-configs <LIST>")]
        [Description("MCP config IDs to include (repeat or comma-separated).")]
        public string[]? McpConfigs { get; init; }

        /// <summary>Print the plan without executing.</summary>
        [CommandOption("--dry-run")]
        [Description("Print the plan without making any changes.")]
        public bool DryRun { get; init; }

        /// <summary>Overwrite existing files without prompting.</summary>
        [CommandOption("-f|--force")]
        [Description("Overwrite existing target files without prompting.")]
        public bool Force { get; init; }

        /// <summary>
        /// How to handle MCP server name conflicts during merge: <c>fail</c> (default), <c>overwrite</c>, or <c>skip</c>.
        /// When set, this takes precedence over <c>--force</c> for the MCP merge step only.
        /// </summary>
        [CommandOption("--mcp-on-conflict <MODE>")]
        [Description("How to handle MCP server name conflicts: fail (default), overwrite, skip.")]
        public string? McpOnConflict { get; init; }
    }
}
