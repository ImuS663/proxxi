using Microsoft.Extensions.Options;

using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.PluginLoaders;
using Proxxi.Plugin.Sdk.ProxySources;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginInfo;

public sealed class PluginInfoCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
) : Command<PluginInfoCommand.PluginInfoCommandSettings>
{
    public class PluginInfoCommandSettings : PluginCommandSettings;

    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, PluginInfoCommandSettings settings, CancellationToken ct)
    {
        var config = configProvider.Get(settings.Id);

        if (config == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not installed.");

        var fullPath = Path.Combine(_pathOptions.PluginsDir, config.Path);

        var descriptor = pluginLoader.LoadPlugins([fullPath])
            .FirstOrDefault(pd => StringComparer.OrdinalIgnoreCase.Equals(pd.Id, config.Id));

        if (descriptor == null)
            throw new InvalidOperationException($"Plugin '{settings.Id}' is not loaded.");

        var grid = new Grid();
        grid.AddColumns(2);

        var id = string.Concat(descriptor.Id, " (", config.Enabled ? "[green]enabled[/]" : "[red]disabled[/]", ")");
        var desc = !string.IsNullOrWhiteSpace(descriptor.Description) ? descriptor.Description : "[grey]<none>[/]";

        var supportBatchMode = !descriptor.HideBatch &&
                               descriptor.ProxySourceType.IsAssignableTo(typeof(IBatchProxySource));

        var supportStreamMode = !descriptor.HideStream &&
                                descriptor.ProxySourceType.IsAssignableTo(typeof(IStreamProxySource));

        console.MarkupLine($"[bold underline]{descriptor.Name}[/]");

        grid.AddRow("[cyan]Id[/]:", id);
        grid.AddRow("[cyan]Alias[/]:", config.Alias ?? "[grey]<none>[/]");
        grid.AddRow("[cyan]Description[/]:", desc);
        grid.AddRow("[cyan]Path[/]:", descriptor.Path);
        grid.AddRow("[cyan]Version[/]:", descriptor.Version);
        grid.AddRow("[cyan]Batch mode[/]:", supportBatchMode ? "[green]yes[/]" : "[red]no[/]");
        grid.AddRow("[cyan]Stream mode[/]:", supportStreamMode ? "[green]yes[/]" : "[red]no[/]");

        console.Write(grid);

        return 0;
    }
}