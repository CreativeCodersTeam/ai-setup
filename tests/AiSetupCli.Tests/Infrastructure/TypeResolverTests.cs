using AiSetup.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class TypeResolverTests
{
    [Fact]
    public void Resolve_WithRegisteredType_ReturnsInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IFoo, Foo>();
        var sut = new TypeResolver(services.BuildServiceProvider());

        // Act
        var result = sut.Resolve(typeof(IFoo));

        // Assert
        result.Should().BeOfType<Foo>();
    }

    [Fact]
    public void Resolve_WithUnknownType_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = new TypeResolver(services.BuildServiceProvider());

        // Act
        var result = sut.Resolve(typeof(IFoo));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Resolve_WithNullType_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = new TypeResolver(services.BuildServiceProvider());

        // Act
        var result = sut.Resolve(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Dispose_WhenCalled_DisposesUnderlyingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<DisposableTracker>();
        var provider = services.BuildServiceProvider();
        var tracker = provider.GetRequiredService<DisposableTracker>();
        var sut = new TypeResolver(provider);

        // Act
        sut.Dispose();

        // Assert
        tracker.Disposed.Should().BeTrue();
    }

    private interface IFoo
    {
    }

    private sealed class Foo : IFoo
    {
    }

    private sealed class DisposableTracker : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }
}
