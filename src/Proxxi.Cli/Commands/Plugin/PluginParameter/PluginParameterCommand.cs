using Microsoft.Extensions.Options;

using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameter;

public class PluginParameterCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
) : Command<PluginParameterCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, PluginParameterCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
        {
            console.MarkupLine($"[red]Plugin '{settings.Id}' is not installed.[/]");
            return 1;
        }

        var fullPath = Path.Combine(_pathOptions.PluginsDir, config.Path);

        var descriptor = pluginLoader.LoadPlugins([fullPath])
            .FirstOrDefault(pd => StringComparer.OrdinalIgnoreCase.Equals(pd.Id, config.Id));

        if (descriptor == null)
        {
            console.MarkupLine($"[red]Plugin '{settings.Id}' is not loaded.[/]");
            return 1;
        }

        string? parameterName;

        if (settings.Force)
            parameterName = settings.Name;
        else
            parameterName = descriptor.Parameters
                .FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, settings.Name))?.Name;

        if (parameterName == null)
        {
            console.MarkupLine($"[red]Plugin does not support parameter '{settings.Name}'.[/]");
            return 1;
        }

        if (settings.Value != null)
        {
            config.Parameters[parameterName] = settings.Value;

            configProvider.Upsert(config);
            configProvider.Save();

            console.MarkupLine($"[green]✓[/] Parameter '{parameterName}' set to [yellow]{settings.Value}[/]");

            return 0;
        }

        if (settings.Remove)
        {
            if (!config.Parameters.Remove(parameterName))
            {
                console.MarkupLine("[yellow]Plugin has no parameter to remove.[/]");
                return 0;
            }

            configProvider.Upsert(config);
            configProvider.Save();

            console.MarkupLine("[green]✓[/] Parameter removed.");

            return 0;
        }

        console.MarkupLine(config.Parameters.TryGetValue(parameterName, out var value)
            ? $"Parameter '{parameterName}': [yellow]{value}[/]"
            : $"[yellow]Parameter '{parameterName}' is not set.[/]");

        return 0;
    }

    public override ValidationResult Validate(CommandContext context, PluginParameterCommandSettings settings)
    {
        if (settings.Value != null && settings.Remove)
            return ValidationResult.Error("Cannot specify [VALUE] and --remove together.");

        return ValidationResult.Success();
    }
}