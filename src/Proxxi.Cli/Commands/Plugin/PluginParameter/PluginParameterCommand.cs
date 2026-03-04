using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameter;

public sealed class PluginParameterCommand(
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
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not installed.");

        var fullPath = Path.Combine(_pathOptions.PluginsDir, config.Path);

        var descriptor = pluginLoader.LoadPlugin(fullPath, config.Id);

        if (descriptor == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not loaded.");

        string? parameterName = settings.Force ? settings.Name : ExtractParameterName(settings, descriptor);

        if (parameterName == null)
            throw new InvalidOperationException($"Plugin does not support parameter '{settings.Name}'.");

        if (settings.Value != null)
            return SetParameter(settings.Value, config, parameterName);

        if (settings.Remove)
            return RemovePluginParameter(config, parameterName);

        console.MarkupLine(config.Parameters.TryGetValue(parameterName, out var value)
            ? $"Parameter '{parameterName}': [yellow]{value}[/]"
            : $"[blue]Info:[/] Parameter '{parameterName}' is not set.");

        return 0;
    }

    public override ValidationResult Validate(CommandContext context, PluginParameterCommandSettings settings)
    {
        if (settings.Value != null && settings.Remove)
            return ValidationResult.Error("Cannot specify [VALUE] and --remove together.");

        return ValidationResult.Success();
    }

    private static string? ExtractParameterName(PluginParameterCommandSettings settings, PluginDescriptor descriptor) =>
        descriptor.Parameters.FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, settings.Name))?.Name;

    private int SetParameter(string value, PluginConfig config, string parameterName)
    {
        config.Parameters[parameterName] = value;

        UpdatePluginConfig(config);

        console.MarkupLine($"[green]✓[/] Parameter '{parameterName}' set to [yellow]{value}[/]");

        return 0;
    }

    private int RemovePluginParameter(PluginConfig config, string parameterName)
    {
        if (!config.Parameters.Remove(parameterName))
        {
            console.MarkupLine("[blue]Info:[/] Plugin has no parameter to remove.");
            return 0;
        }

        UpdatePluginConfig(config);

        console.MarkupLine("[green]✓[/] Parameter removed.");

        return 0;
    }

    private void UpdatePluginConfig(PluginConfig config)
    {
        configProvider.Upsert(config);
        configProvider.Save();
    }
}