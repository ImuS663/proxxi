using Proxxi.Core.Providers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginDisable;

public class PluginDisableCommand(IAnsiConsole console, IPluginConfigProvider configProvider)
    : Command<PluginDisableCommand.PluginDisableCommandSettings>
{
    public class PluginDisableCommandSettings : PluginCommandSettings;

    public override int Execute(CommandContext context, PluginDisableCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' not found.");

        if (!config.Enabled)
        {
            console.MarkupLine("[blue]Info:[/] Plugin already disabled.");
            return 0;
        }

        config.Enabled = false;

        configProvider.Upsert(config);
        configProvider.Save();

        console.MarkupLine("[green]✓[/] Plugin disabled.");
        return 0;
    }
}