using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginAlias;

public class PluginAliasCommandSettings : PluginCommandSettings
{
    [CommandArgument(0, "[VALUE]")]
    [Description("The alias to set for the plugin (if omitted, the print the current alias)")]
    public string? Value { get; init; }

    [CommandOption("-r|--remove")]
    [Description("Remove the alias for the plugin (if set)")]
    public bool Remove { get; init; }
}