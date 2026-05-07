using AiSetup.Cli.Commands;
using AiSetup.Cli.Infrastructure;
using AiSetup.Cli.Rendering;
using AiSetup.Lib.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        using var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                standardErrorFromLevel: Serilog.Events.LogEventLevel.Warning)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        services.AddSingleton<PlanRenderer>();
        services.AddLogging(builder => builder.AddSerilog(logger, dispose: false));
        services.AddAiSetupLib();

        var registrar = new TypeRegistrar(services);
        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("ai-setup");

            config.AddCommand<DeployCommand>("deploy")
                .WithDescription("Deploy AI assets to a target repository or local install.");

            config.AddCommand<ListCommand>("list")
                .WithDescription("List discovered assets or profiles.");

            config.AddCommand<InfoCommand>("info")
                .WithDescription("Show details for a single asset.");
        });

        return app.Run(args);
    }
}
