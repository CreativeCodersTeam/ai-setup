using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiSetupCli.Infrastructure;

internal sealed class SpectreTypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public SpectreTypeRegistrar(IServiceCollection services) => _services = services;

    public ITypeResolver Build() => new SpectreTypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation)
        => _services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation)
        => _services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> func)
        => _services.AddSingleton(service, _ => func());
}
