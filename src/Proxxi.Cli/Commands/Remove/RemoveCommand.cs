using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Remove;

public sealed class RemoveCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IOptions<ProxxiPathsOptions> options
) : Command<RemoveCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, RemoveCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not installed.");

        var allPlugins = configProvider.GetAll().Where(c => c.Path == config.Path).ToList();

        var pluginIds = string.Join(", ", allPlugins.Select(c => c.Id));

        if (settings.Yes || console.Confirm($"Are you sure you want to remove {pluginIds}?", false))
        {
            var targetDir = Path.Combine(_pathOptions.PluginsDir, Path.GetDirectoryName(config.Path) ?? string.Empty);

            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            RemovePluginsFromConfig(allPlugins);

            console.MarkupLine("[green]✓[/] Successfully removed plugins and associated files.");
        }
        else
        {
            console.MarkupLine("[yellow]Removal aborted.[/]");
        }

        return 0;
    }

    private void RemovePluginsFromConfig(List<PluginConfig> allPlugins)
    {
        foreach (var plugin in allPlugins)
            configProvider.Remove(plugin.Id);

        configProvider.Save();
    }
}