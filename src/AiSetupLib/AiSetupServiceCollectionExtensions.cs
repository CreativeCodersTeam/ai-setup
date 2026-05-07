using AiSetup.Aggregation;
using AiSetup.Deploy;
using AiSetup.Discovery;
using AiSetup.Platform;
using AiSetup.Profiles;
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
    /// Registers all AiSetupLib services. The caller must register an
    /// <see cref="IAssetRepository"/> and <see cref="IProfileRepository"/> bound to a concrete
    /// repository root (or use <see cref="AddAiSetupForRepo(IServiceCollection, string)"/>).
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
        services.AddSingleton<IProfileResolver, ProfileResolver>();
        services.AddSingleton<IDeployService, DeployService>();

        return services;
    }

    /// <summary>
    /// Convenience overload that also binds <see cref="IAssetRepository"/> and
    /// <see cref="IProfileRepository"/> to a fixed source repository path.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="sourceRepoPath">Absolute path of the ai-setup repository.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddAiSetupForRepo(this IServiceCollection services, string sourceRepoPath)
    {
        Ensure.NotNull(services);
        Ensure.IsNotNullOrWhitespace(sourceRepoPath);

        services.AddAiSetup();
        services.AddSingleton<IAssetRepository>(sp =>
            new FileSystemAssetRepository(sp.GetRequiredService<IFileSystem>(), sourceRepoPath));
        services.AddSingleton<IProfileRepository>(sp =>
            new YamlProfileRepository(sp.GetRequiredService<IFileSystem>(), sourceRepoPath));

        return services;
    }
}
