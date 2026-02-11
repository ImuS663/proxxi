using Microsoft.Extensions.Options;

using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameters;

public class PluginParametersCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
) : Command<PluginParametersCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, PluginParametersCommandSettings settings,
        CancellationToken cancellationToken)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
        {
            console.MarkupLine($"[red]Plugin '{settings.Id}' is not installed.[/]");
            return 1;
        }

        if (settings.Description)
        {
            var fullPath = Path.Combine(_pathOptions.PluginsDir, config.Path);

            var descriptor = pluginLoader.LoadPlugins([fullPath])
                .FirstOrDefault(pd => StringComparer.OrdinalIgnoreCase.Equals(pd.Id, config.Id));

            if (descriptor == null)
            {
                console.MarkupLine($"[red]Plugin '{settings.Id}' is not loaded.[/]");
                return 1;
            }

            console.MarkupLine("[bold underline]Supported parameters:[/]");

            if (descriptor.Parameters.Count == 0)
            {
                console.MarkupLine("[yellow]Plugin has no declared parameters.[/]");
                return 0;
            }

            var grid = new Grid();
            grid.AddColumns(3);

            foreach (var parameter in descriptor.Parameters)
            {
                var description = parameter.Description;
                var name = string.Concat("[cyan]", parameter.Name, "[/]", ':');

                if (parameter.Required)
                    description = string.Concat(description, " [red](required)[/]");

                grid.AddRow("", name, description);
            }


            console.Write(grid);
        }
        else
        {
            console.MarkupLine("[bold underline]Configured parameters:[/]");

            if (config.Parameters.Count == 0)
            {
                console.MarkupLine("[yellow]No parameters configured for this plugin.[/]");
                return 0;
            }

            var grid = new Grid();
            grid.AddColumns(3);

            foreach ((string key, string value) in config.Parameters)
            {
                var name = string.Concat("[cyan]", key, "[/]", ':');

                grid.AddRow("", name, value);
            }


            console.Write(grid);
        }

        return 0;
    }
}