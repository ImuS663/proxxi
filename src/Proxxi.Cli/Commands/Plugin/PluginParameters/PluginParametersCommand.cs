using Microsoft.Extensions.Options;

using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameters;

public sealed class PluginParametersCommand(
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
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not installed.");

        if (settings.Description)
        {
            var fullPath = Path.Combine(_pathOptions.PluginsDir, config.Path);

            var descriptor = pluginLoader.LoadPlugins([fullPath])
                .FirstOrDefault(pd => StringComparer.OrdinalIgnoreCase.Equals(pd.Id, config.Id));

            if (descriptor == null)
                throw new InvalidOperationException($"Plugin '{settings.Id}' is not loaded.");

            console.MarkupLine("[bold underline]Supported parameters:[/]");

            if (descriptor.Parameters.Count == 0)
            {
                console.MarkupLine("[blue]Info:[/] Plugin has no declared parameters.");
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
                console.MarkupLine("[blue]Info:[/] No parameters configured for this plugin.");
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