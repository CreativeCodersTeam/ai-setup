using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Configuration;
using AiSetup.Lib.Deploy;
using AiSetup.Lib.Discovery;
using AiSetup.Lib.IO;
using AiSetup.Lib.Profiles;
using AiSetup.Lib.Targets;
using AiSetup.Lib.Targets.Platform;
using CreativeCoders.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup.Lib.DependencyInjection;

/// <summary>
/// DI extension methods that register all AiSetup library services.
/// </summary>
public static class AiSetupLibServiceCollectionExtensions
{
    /// <summary>
    /// Registers the full set of AiSetup services and the default
    /// <see cref="PathOptions"/>.
    /// </summary>
    public static IServiceCollection AddAiSetupLib(this IServiceCollection services)
    {
        Ensure.NotNull(services, nameof(services));

        services.AddOptions<PathOptions>();

        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IFrontmatterParser, FrontmatterParser>();
        services.AddSingleton<IAssetDiscovery, AssetDiscoveryService>();
        services.AddSingleton<RepoRootResolver>();
        services.AddSingleton<IProfileResolver, ProfileResolver>();
        services.AddSingleton<IContentAggregator, MarkdownAggregator>();
        services.AddSingleton<IMcpConfigMerger, McpConfigMerger>();
        services.AddSingleton<IPlatformPathProvider, PlatformPathProvider>();

        services.AddSingleton<IDeployTarget, CopilotCliTarget>();
        services.AddSingleton<IDeployTarget, ClaudeCodeTarget>();
        services.AddSingleton<TargetRegistry>();
        services.AddSingleton<IDeployService, DeployService>();

        return services;
    }
}
