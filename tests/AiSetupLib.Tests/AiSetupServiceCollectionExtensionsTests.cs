using AiSetup.Aggregation;
using AiSetup.Discovery;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup.Tests;

public sealed class AiSetupServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAiSetup_WhenCalled_RegistersCorePlatformServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IFileSystem>().Should().BeOfType<FileSystem>();
        provider.GetService<IPathProvider>().Should().BeOfType<PathProvider>();
        provider.GetService<IMarkdownAggregator>().Should().BeOfType<MarkdownAggregator>();
        provider.GetService<IMcpConfigMerger>().Should().BeOfType<McpConfigMerger>();
    }

    [Fact]
    public void AddAiSetup_WhenCalled_RegistersBothDeployTargets()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetup();
        var provider = services.BuildServiceProvider();
        var targets = provider.GetServices<IDeployTarget>().ToArray();

        // Assert
        targets.Should().HaveCount(2);
        targets.Select(t => t.GetType()).Should().BeEquivalentTo(new[]
        {
            typeof(CopilotCliTarget),
            typeof(ClaudeCodeTarget)
        });
    }

    [Fact]
    public void AddAiSetup_WhenCalled_RegistersTargetRegistry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<ITargetRegistry>().Should().BeOfType<TargetRegistry>();
    }

    [Fact]
    public void AddAiSetup_WhenCalled_RegistersRepositoryFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetup();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IRepositoryFactory>().Should().BeOfType<RepositoryFactory>();
    }

    [Fact]
    public void RepositoryFactory_CreateAssets_ReturnsFileSystemAssetRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAiSetup();
        var factory = services.BuildServiceProvider().GetRequiredService<IRepositoryFactory>();

        // Act
        var repo = factory.CreateAssets("/some/repo");

        // Assert
        repo.Should().BeOfType<FileSystemAssetRepository>();
    }

    [Fact]
    public void RepositoryFactory_CreateProfiles_ReturnsYamlProfileRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAiSetup();
        var factory = services.BuildServiceProvider().GetRequiredService<IRepositoryFactory>();

        // Act
        var repo = factory.CreateProfiles("/some/repo");

        // Assert
        repo.Should().BeOfType<YamlProfileRepository>();
    }

    [Fact]
    public void AddAiSetup_WithNullServices_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => AiSetupServiceCollectionExtensions.AddAiSetup(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAiSetup_WhenCalled_ReturnsSameCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAiSetup();

        // Assert
        result.Should().BeSameAs(services);
    }
}
