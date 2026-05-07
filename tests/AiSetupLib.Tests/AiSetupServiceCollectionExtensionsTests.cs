using AiSetup.Aggregation;
using AiSetup.Deploy;
using AiSetup.Discovery;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup.Tests;

public sealed class AiSetupServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAiSetup_RegistersCorePlatformServices()
    {
        var services = new ServiceCollection();

        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        provider.GetService<IFileSystem>().Should().BeOfType<FileSystem>();
        provider.GetService<IPathProvider>().Should().BeOfType<PathProvider>();
        provider.GetService<IMarkdownAggregator>().Should().BeOfType<MarkdownAggregator>();
        provider.GetService<IMcpConfigMerger>().Should().BeOfType<McpConfigMerger>();
    }

    [Fact]
    public void AddAiSetup_RegistersBothDeployTargets()
    {
        var services = new ServiceCollection();

        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        var targets = provider.GetServices<IDeployTarget>().ToArray();

        targets.Should().HaveCount(2);
        targets.Select(t => t.GetType()).Should().BeEquivalentTo(new[]
        {
            typeof(CopilotCliTarget),
            typeof(ClaudeCodeTarget)
        });
    }

    [Fact]
    public void AddAiSetup_RegistersTargetRegistry()
    {
        var services = new ServiceCollection();

        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        provider.GetService<ITargetRegistry>().Should().BeOfType<TargetRegistry>();
    }

    [Fact]
    public void AddAiSetup_RegistersDescriptorsForResolverAndService()
    {
        var services = new ServiceCollection();

        services.AddAiSetup();

        services.Should().Contain(d => d.ServiceType == typeof(IProfileResolver));
        services.Should().Contain(d => d.ServiceType == typeof(IDeployService));
    }

    [Fact]
    public void AddAiSetup_NullServices_Throws()
    {
        Action act = () => AiSetupServiceCollectionExtensions.AddAiSetup(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAiSetupForRepo_BindsRepositoriesAndServices()
    {
        var services = new ServiceCollection();

        services.AddAiSetupForRepo("/some/repo");
        var provider = services.BuildServiceProvider();

        provider.GetService<IAssetRepository>().Should().BeOfType<FileSystemAssetRepository>();
        provider.GetService<IProfileRepository>().Should().BeOfType<YamlProfileRepository>();
        provider.GetService<IProfileResolver>().Should().BeOfType<ProfileResolver>();
        provider.GetService<IDeployService>().Should().BeOfType<DeployService>();
    }

    [Fact]
    public void AddAiSetupForRepo_NullServices_Throws()
    {
        Action act = () => AiSetupServiceCollectionExtensions.AddAiSetupForRepo(null!, "/repo");

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAiSetupForRepo_InvalidPath_Throws(string? path)
    {
        var services = new ServiceCollection();

        Action act = () => services.AddAiSetupForRepo(path!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAiSetup_ReturnsSameCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddAiSetup();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAiSetupForRepo_ReturnsSameCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddAiSetupForRepo("/repo");

        result.Should().BeSameAs(services);
    }
}
