using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameter;

public sealed class PluginParameterCommandSettings : PluginCommandSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("The parameter name to set for the plugin")]
    public required string Name { get; init; }

    [CommandArgument(1, "[VALUE]")]
    [Description("The value to set for the parameter (if omitted, the print the current value)")]
    public string? Value { get; init; }

    [CommandOption("-r|--remove")]
    [Description("Remove the parameter for the plugin (if set)")]
    public bool Remove { get; init; }

    [CommandOption("-f|--force")]
    [Description("Allow setting or removing parameters not declared by the plugin")]
    public bool Force { get; init; }
}