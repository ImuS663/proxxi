using Proxxi.Core.Providers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginEnable;

public class PluginEnableCommand(IAnsiConsole console, IPluginConfigProvider configProvider)
    : Command<PluginEnableCommand.PluginEnableCommandSettings>
{
    public class PluginEnableCommandSettings : PluginCommandSettings;

    public override int Execute(CommandContext context, PluginEnableCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' not found.");

        if (config.Enabled)
        {
            console.MarkupLine("[blue]Info:[/] Plugin already enabled.");
            return 0;
        }

        config.Enabled = true;

        configProvider.Upsert(config);
        configProvider.Save();

        console.MarkupLine("[green]✓[/] Plugin enabled.");
        return 0;
    }
}