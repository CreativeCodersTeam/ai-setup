using Spectre.Console.Cli;

namespace AiSetupCli.Infrastructure;

internal sealed class SpectreTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public SpectreTypeResolver(IServiceProvider provider) => _provider = provider;

    public object? Resolve(Type? type) => type is null ? null : _provider.GetService(type);

    public void Dispose()
    {
        if (_provider is IDisposable d) d.Dispose();
    }
}
