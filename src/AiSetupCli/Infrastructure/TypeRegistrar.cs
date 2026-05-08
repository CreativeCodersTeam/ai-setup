using CreativeCoders.Core;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiSetup.Cli.Infrastructure;

/// <summary>
/// Bridges <see cref="Spectre.Console.Cli.ITypeRegistrar"/> to
/// <see cref="Microsoft.Extensions.DependencyInjection"/>.
/// </summary>
internal sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = Ensure.NotNull(builder);
    }

    public ITypeResolver Build() => new TypeResolver(_builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) =>
        _builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) =>
        _builder.AddSingleton(service, _ => factory());
}
