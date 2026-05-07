using AiSetup.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class TypeResolverTests
{
    [Fact]
    public void Resolve_RegisteredType_ReturnsInstance()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFoo, Foo>();
        var sut = new TypeResolver(services.BuildServiceProvider());

        sut.Resolve(typeof(IFoo)).Should().BeOfType<Foo>();
    }

    [Fact]
    public void Resolve_UnknownType_ReturnsNull()
    {
        var services = new ServiceCollection();
        var sut = new TypeResolver(services.BuildServiceProvider());

        sut.Resolve(typeof(IFoo)).Should().BeNull();
    }

    [Fact]
    public void Resolve_NullType_ReturnsNull()
    {
        var services = new ServiceCollection();
        var sut = new TypeResolver(services.BuildServiceProvider());

        sut.Resolve(null).Should().BeNull();
    }

    [Fact]
    public void Dispose_DisposesUnderlyingProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DisposableTracker>();
        var provider = services.BuildServiceProvider();
        var tracker = provider.GetRequiredService<DisposableTracker>();
        var sut = new TypeResolver(provider);

        sut.Dispose();

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
