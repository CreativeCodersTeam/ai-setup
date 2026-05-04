using AiSetupCli.Commands;
using AiSetupCli.Infrastructure;
using AiSetupLib;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var repoRoot = ResolveRepoRoot();

var services = new ServiceCollection();
services.AddAiSetup(repoRoot);
services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);

var registrar = new SpectreTypeRegistrar(services);
var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("ai-setup");
    config.AddCommand<DeployCommand>("deploy");
    config.AddCommand<ListCommand>("list");
    config.AddCommand<InfoCommand>("info");
});

return await app.RunAsync(args);

static string ResolveRepoRoot()
{
    var env = Environment.GetEnvironmentVariable("AI_SETUP_REPO_ROOT");
    if (!string.IsNullOrEmpty(env)) return env;

    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "agents"))
            && Directory.Exists(Path.Combine(dir.FullName, "skills")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return Directory.GetCurrentDirectory();
}
