using AiSetup.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class TypeRegistrarTests
{
    [Fact]
    public void Register_WhenCalled_AddsServiceToContainer()
    {
        // Arrange
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);

        // Act
        sut.Register(typeof(IFoo), typeof(Foo));

        // Assert
        collection.Should().Contain(d => d.ServiceType == typeof(IFoo));
    }

    [Fact]
    public void RegisterInstance_WhenCalled_AddsSingletonInstance()
    {
        // Arrange
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        var instance = new Foo();

        // Act
        sut.RegisterInstance(typeof(IFoo), instance);

        // Assert
        var resolver = sut.Build();
        resolver.Resolve(typeof(IFoo)).Should().BeSameAs(instance);
    }

    [Fact]
    public void RegisterLazy_WhenResolving_InvokesFactoryOnce()
    {
        // Arrange
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        var produced = new Foo();
        var invoked = 0;

        sut.RegisterLazy(typeof(IFoo), () =>
        {
            invoked++;
            return produced;
        });

        // Act
        var resolver = sut.Build();
        var resolved = resolver.Resolve(typeof(IFoo));

        // Assert
        resolved.Should().BeSameAs(produced);
        invoked.Should().Be(1);
    }

    [Fact]
    public void Build_WithRegisteredService_ReturnsResolverThatResolvesIt()
    {
        // Arrange
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        sut.Register(typeof(IFoo), typeof(Foo));

        // Act
        ITypeResolver resolver = sut.Build();

        // Assert
        resolver.Should().NotBeNull();
        resolver.Resolve(typeof(IFoo)).Should().BeOfType<Foo>();
    }

    private interface IFoo
    {
    }

    private sealed class Foo : IFoo
    {
    }
}
