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
    public void AddAiSetup_WhenCalled_RegistersDescriptorsForResolverAndService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetup();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(IProfileResolver));
        services.Should().Contain(d => d.ServiceType == typeof(IDeployService));
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
    public void AddAiSetupForRepo_WhenCalled_BindsRepositoriesAndServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAiSetupForRepo("/some/repo");
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IAssetRepository>().Should().BeOfType<FileSystemAssetRepository>();
        provider.GetService<IProfileRepository>().Should().BeOfType<YamlProfileRepository>();
        provider.GetService<IProfileResolver>().Should().BeOfType<ProfileResolver>();
        provider.GetService<IDeployService>().Should().BeOfType<DeployService>();
    }

    [Fact]
    public void AddAiSetupForRepo_WithNullServices_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => AiSetupServiceCollectionExtensions.AddAiSetupForRepo(null!, "/repo");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAiSetupForRepo_WithInvalidPath_ThrowsArgumentException(string? path)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddAiSetupForRepo(path!);

        // Assert
        act.Should().Throw<ArgumentException>();
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

    [Fact]
    public void AddAiSetupForRepo_WhenCalled_ReturnsSameCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAiSetupForRepo("/repo");

        // Assert
        result.Should().BeSameAs(services);
    }
}
