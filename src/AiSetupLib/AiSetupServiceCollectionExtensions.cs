using AiSetup.Aggregation;
using AiSetup.Platform;
using AiSetup.Targets;
using CreativeCoders.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup;

/// <summary>
/// DI registration helpers for AiSetupLib.
/// </summary>
public static class AiSetupServiceCollectionExtensions
{
    /// <summary>
    /// Registers all AiSetupLib services. Asset and profile repositories are constructed per-call
    /// via <see cref="IRepositoryFactory"/>, since they are bound to a source repository path
    /// supplied at runtime (e.g. from a CLI option).
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddAiSetup(this IServiceCollection services)
    {
        Ensure.NotNull(services);

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IPathProvider, PathProvider>();
        services.AddSingleton<IMarkdownAggregator, MarkdownAggregator>();
        services.AddSingleton<IMcpConfigMerger, McpConfigMerger>();
        services.AddSingleton<IDeployTarget, CopilotCliTarget>();
        services.AddSingleton<IDeployTarget, ClaudeCodeTarget>();
        services.AddSingleton<ITargetRegistry, TargetRegistry>();
        services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

        return services;
    }
}
