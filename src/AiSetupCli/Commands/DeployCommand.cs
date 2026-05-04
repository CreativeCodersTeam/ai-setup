using System.ComponentModel;
using AiSetupCli.Output;
using AiSetupLib.Deploy;
using AiSetupLib.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class DeployCommand : Command<DeployCommand.Settings>
{
    private readonly IDeployService _service;
    private readonly IAnsiConsole _console;

    public DeployCommand(IDeployService service, IAnsiConsole console)
    {
        _service = service;
        _console = console;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--target <TARGET>")]
        [Description("copilot-cli | claude-code")]
        public string Target { get; init; } = "";

        [CommandOption("--mode <MODE>")]
        [Description("repo | local")]
        public string Mode { get; init; } = "";

        [CommandOption("--repo <PATH>")]
        public string? RepoPath { get; init; }

        [CommandOption("--profile <NAME>")]
        public string? Profile { get; init; }

        [CommandOption("--agents <CSV>")]
        public string? Agents { get; init; }

        [CommandOption("--skills <CSV>")]
        public string? Skills { get; init; }

        [CommandOption("--instructions <CSV>")]
        public string? Instructions { get; init; }

        [CommandOption("--mcp-configs <CSV>")]
        public string? McpConfigs { get; init; }

        [CommandOption("--dry-run")]
        public bool DryRun { get; init; }

        [CommandOption("--force")]
        public bool Force { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!TryParseTarget(settings.Target, out var target))
        {
            _console.MarkupLine($"[red]Unknown --target: {Markup.Escape(settings.Target)}[/]");
            return 2;
        }
        if (!TryParseMode(settings.Mode, out var mode))
        {
            _console.MarkupLine($"[red]Unknown --mode: {Markup.Escape(settings.Mode)}[/]");
            return 2;
        }

        var dest = mode == DeployMode.Repo
            ? settings.RepoPath ?? Directory.GetCurrentDirectory()
            : "";

        var options = new DeployOptions(target, mode, dest)
        {
            Profile = settings.Profile,
            Agents = Csv(settings.Agents),
            Skills = Csv(settings.Skills),
            Instructions = Csv(settings.Instructions),
            McpConfigs = Csv(settings.McpConfigs),
            DryRun = settings.DryRun,
            Force = settings.Force,
        };

        try
        {
            var plan = _service.Deploy(options);
            DryRunRenderer.Render(_console, plan);
            if (settings.DryRun)
                _console.MarkupLine("[grey]Dry run — nothing was written.[/]");
            return 0;
        }
        catch (AssetNotFoundException ex)
        {
            _console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            _console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
    }

    private static IReadOnlyList<string> Csv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool TryParseTarget(string s, out DeployTarget t)
    {
        t = default;
        return s switch
        {
            "copilot-cli" => (t = DeployTarget.CopilotCli) == DeployTarget.CopilotCli,
            "claude-code" => (t = DeployTarget.ClaudeCode) == DeployTarget.ClaudeCode,
            _ => false,
        };
    }

    private static bool TryParseMode(string s, out DeployMode m)
    {
        m = default;
        return s switch
        {
            "repo" => (m = DeployMode.Repo) == DeployMode.Repo,
            "local" => (m = DeployMode.Local) == DeployMode.Local,
            _ => false,
        };
    }
}
