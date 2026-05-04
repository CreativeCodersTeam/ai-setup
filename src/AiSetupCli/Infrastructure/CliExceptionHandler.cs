using AiSetupLib.Deploy;
using Spectre.Console;

namespace AiSetupCli.Infrastructure;

internal static class CliExceptionHandler
{
    public static int Run(IAnsiConsole console, Func<int> action)
    {
        try
        {
            return action();
        }
        catch (AssetNotFoundException ex)
        {
            console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            console.MarkupLine("[red]Permission denied: " + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (IOException ex)
        {
            console.MarkupLine("[red]I/O error: " + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (Exception ex) when (ex.GetType().Namespace?.StartsWith("YamlDotNet", StringComparison.Ordinal) == true)
        {
            console.MarkupLine("[red]YAML error: " + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (Exception ex)
        {
            console.MarkupLine("[red]" + Markup.Escape(ex.GetType().Name + ": " + ex.Message) + "[/]");
            return 1;
        }
    }
}
