using Microsoft.Extensions.Options;

using Proxxi.Core.Options;
using Proxxi.Core.Providers;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugins;

public class PluginsCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IOptions<ProxxiPathsOptions> options
)
    : Command<PluginsCommand.PluginsCommandSettings>
{
    public class PluginsCommandSettings : CommandSettings;

    private static readonly Style HeaderStyle = new(decoration: Decoration.Bold);

    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, PluginsCommandSettings settings, CancellationToken ct)
    {
        var configs = configProvider.GetAll();

        if (configs.Count == 0)
        {
            console.MarkupLine("[yellow]No plugins installed.[/]");
            return 0;
        }

        var table = new Table();

        table.Border(TableBorder.Markdown);

        table.AddColumn(new TableColumn(new Text("ID", HeaderStyle)));
        table.AddColumn(new TableColumn(new Text("Alias", HeaderStyle)));
        table.AddColumn(new TableColumn(new Text("Version", HeaderStyle)));
        table.AddColumn(new TableColumn(new Text("Status", HeaderStyle)));

        foreach (var config in configs)
        {
            string status;

            var fileExists = File.Exists(Path.Combine(_pathOptions.PluginsDir, config.Path));

            if (!fileExists)
                status = "[red]missing[/]";
            else if (config.Enabled)
                status = "[green]enabled[/]";
            else
                status = "[red]disabled[/]";

            table.AddRow(config.Id, config.Alias ?? "[grey]<none>[/]", config.Version, status);
        }

        console.Write(table);

        return 0;
    }
}