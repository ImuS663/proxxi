using Proxxi.Core.Models;
using Proxxi.Core.Providers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginAlias;

public sealed class PluginAliasCommand(IAnsiConsole console, IPluginConfigProvider configProvider)
    : Command<PluginAliasCommandSettings>
{
    public override int Execute(CommandContext context, PluginAliasCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not installed.");

        if (settings.Value != null)
            return SetAlias(settings.Value, config);

        if (settings.Remove)
            return RemovePluginAlias(config);

        console.MarkupLine(config.Alias != null
            ? $"Alias: [yellow]{config.Alias}[/]"
            : "[blue]Info:[/] No alias is set for this plugin.");

        return 0;
    }

    public override ValidationResult Validate(CommandContext context, PluginAliasCommandSettings settings)
    {
        if (settings.Value != null && settings.Remove)
            return ValidationResult.Error("Cannot specify [VALUE] and --remove together.");

        return ValidationResult.Success();
    }

    private int SetAlias(string value, PluginConfig config)
    {
        if (configProvider.AliasExists(value, config.Id))
            throw new InvalidOperationException($"Alias '{value}' is already in use.");

        var oldAlias = config.Alias;
        config.Alias = value;

        configProvider.UpsertAndSave(config);

        console.MarkupLine($"[green]✓[/] Alias updated: {oldAlias ?? "<none>"} → [yellow]{config.Alias}[/]");
        return 0;
    }

    private int RemovePluginAlias(PluginConfig config)
    {
        if (config.Alias == null)
        {
            console.MarkupLine("[blue]Info:[/] Plugin has no alias to remove.");
            return 0;
        }

        config.Alias = null;

        configProvider.UpsertAndSave(config);

        console.MarkupLine("[green]✓[/] Alias removed.");
        return 0;
    }
}