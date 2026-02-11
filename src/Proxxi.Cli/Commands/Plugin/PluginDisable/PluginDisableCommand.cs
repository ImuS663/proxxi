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
        {
            console.MarkupLine($"[red]Plugin '{settings.Id}' not found.[/]");
            return 1;
        }

        if (!config.Enabled)
        {
            console.MarkupLine("[yellow]Plugin already disabled.[/]");
            return 0;
        }

        config.Enabled = false;

        configProvider.Upsert(config);
        configProvider.Save();

        console.MarkupLine("[green]✓[/] Plugin disabled.");
        return 0;
    }
}