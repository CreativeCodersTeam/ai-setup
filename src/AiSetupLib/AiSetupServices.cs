using System.IO.Abstractions;
using AiSetupLib.Aggregation;
using AiSetupLib.Deploy;
using AiSetupLib.Discovery;
using AiSetupLib.Paths;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib;

public static class AiSetupServices
{
    public static IServiceCollection AddAiSetup(this IServiceCollection services, string repoRoot)
    {
        services.AddSingleton(new RepoRoot(repoRoot));
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<FrontmatterParser>();
        services.AddSingleton<IAssetDiscovery, AssetDiscoveryService>();
        services.AddSingleton<IProfileResolver, ProfileResolver>();
        services.AddSingleton<IPathProvider>(_ => new PathProvider());
        services.AddSingleton<IContentAggregator, MarkdownAggregator>();
        services.AddSingleton<McpConfigMerger>();
        services.AddSingleton<IDeployTarget, CopilotCliTarget>();
        services.AddSingleton<IDeployTarget, ClaudeCodeTarget>();
        services.AddSingleton<TargetRegistry>(sp => new TargetRegistry(sp.GetServices<IDeployTarget>()));
        services.AddSingleton<IDeployService>(sp => new DeployService(
            sp.GetRequiredService<IAssetDiscovery>(),
            sp.GetRequiredService<IProfileResolver>(),
            sp.GetRequiredService<TargetRegistry>(),
            sp.GetRequiredService<RepoRoot>()));
        return services;
    }
}
