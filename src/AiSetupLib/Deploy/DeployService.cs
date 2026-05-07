using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;
using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiSetup.Deploy;

/// <summary>
/// Default <see cref="IDeployService"/>.
/// </summary>
public sealed class DeployService : IDeployService
{
    private readonly IProfileResolver _profileResolver;
    private readonly ITargetRegistry _targetRegistry;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DeployService> _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="profileResolver">Resolves profile + overrides into asset selection.</param>
    /// <param name="targetRegistry">Registry of deploy target adapters.</param>
    /// <param name="fileSystem">File system used during execution.</param>
    /// <param name="logger">Optional logger.</param>
    public DeployService(
        IProfileResolver profileResolver,
        ITargetRegistry targetRegistry,
        IFileSystem fileSystem,
        ILogger<DeployService>? logger = null)
    {
        _profileResolver = Ensure.NotNull(profileResolver);
        _targetRegistry = Ensure.NotNull(targetRegistry);
        _fileSystem = Ensure.NotNull(fileSystem);
        _logger = logger ?? NullLogger<DeployService>.Instance;
    }

    /// <inheritdoc />
    public DeployReport Deploy(DeployOptions options)
    {
        Ensure.NotNull(options);

        var assets = _profileResolver.Resolve(options);
        var target = _targetRegistry.Get(options.Target);
        var plan = target.Plan(options, assets);

        if (options.DryRun)
        {
            _logger.LogInformation("Dry-run: planned {ActionCount} actions for target {Target}.",
                plan.Actions.Count, plan.Target);

            return new DeployReport(plan, [], [], [], DryRun: true);
        }

        return Execute(plan, options.Force);
    }

    private DeployReport Execute(DeployPlan plan, bool force)
    {
        var executed = new List<DeployAction>();
        var skipped = new List<DeployAction>();
        var errors = new List<(DeployAction Action, string Error)>();

        foreach (var action in plan.Actions)
        {
            if (ShouldSkip(action, force))
            {
                skipped.Add(action);
                _logger.LogWarning("Skipped '{Description}' at {Path}: target exists and --force was not set.",
                    action.Description, action.TargetPath);
                continue;
            }

            try
            {
                ExecuteAction(action);
                executed.Add(action);
            }
            catch (Exception ex)
            {
                errors.Add((action, ex.Message));
                _logger.LogError(ex, "Failed to execute action {Description} at {Path}.",
                    action.Description, action.TargetPath);
            }
        }

        return new DeployReport(plan, executed, skipped, errors, DryRun: false);
    }

    private static bool ShouldSkip(DeployAction action, bool force)
    {
        if (force)
        {
            return false;
        }

        return action.Status == DeployActionStatus.Overwrite && action is not BackupFileAction;
    }

    private void ExecuteAction(DeployAction action)
    {
        switch (action)
        {
            case WriteFileAction write:
                _fileSystem.WriteAllText(write.TargetPath, write.Content);
                break;
            case CopyDirectoryAction copy:
                _fileSystem.CopyDirectory(copy.SourcePath, copy.TargetPath);
                break;
            case BackupFileAction backup:
                if (_fileSystem.FileExists(backup.OriginalPath))
                {
                    _fileSystem.CopyFile(backup.OriginalPath, backup.TargetPath);
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown deploy action type: {action.GetType().Name}");
        }
    }
}
