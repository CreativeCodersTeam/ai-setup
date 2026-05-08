using AiSetup;
using AiSetup.Cli.Commands;
using AiSetup.Cli.Infrastructure;
using AiSetup.Cli.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetup.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAiSetup();
        services.AddSingleton(_ => AnsiConsole.Console);
        services.AddSingleton<PlanRenderer>();

        var app = new CommandApp(new TypeRegistrar(services));

        app.Configure(config =>
        {
            config.SetApplicationName("ai-setup");
            config.AddCommand<DeployCommand>("deploy")
                .WithDescription("Deploy assets to a target system.");
            config.AddCommand<ListCommand>("list")
                .WithDescription("List available assets or profiles.");
            config.AddCommand<InfoCommand>("info")
                .WithDescription("Show details about a single asset.");
        });

        return app.Run(args);
    }
}
