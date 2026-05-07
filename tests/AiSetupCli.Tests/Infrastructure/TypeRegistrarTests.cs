using AiSetup.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class TypeRegistrarTests
{
    [Fact]
    public void Register_AddsServiceToContainer()
    {
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);

        sut.Register(typeof(IFoo), typeof(Foo));

        collection.Should().Contain(d => d.ServiceType == typeof(IFoo));
    }

    [Fact]
    public void RegisterInstance_AddsSingletonInstance()
    {
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        var instance = new Foo();

        sut.RegisterInstance(typeof(IFoo), instance);

        var resolver = sut.Build();
        resolver.Resolve(typeof(IFoo)).Should().BeSameAs(instance);
    }

    [Fact]
    public void RegisterLazy_FactoryIsInvokedWhenResolving()
    {
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        var produced = new Foo();
        var invoked = 0;

        sut.RegisterLazy(typeof(IFoo), () =>
        {
            invoked++;
            return produced;
        });

        var resolver = sut.Build();
        resolver.Resolve(typeof(IFoo)).Should().BeSameAs(produced);
        invoked.Should().Be(1);
    }

    [Fact]
    public void Build_ReturnsResolverThatResolvesRegisteredService()
    {
        var collection = new ServiceCollection();
        var sut = new TypeRegistrar(collection);
        sut.Register(typeof(IFoo), typeof(Foo));

        ITypeResolver resolver = sut.Build();

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
