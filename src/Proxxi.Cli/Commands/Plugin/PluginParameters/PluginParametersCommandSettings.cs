using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin.PluginParameters;

public class PluginParametersCommandSettings : PluginCommandSettings
{
    [CommandOption("-d|--desc")]
    [Description("Show plugin supported parameters and their descriptions")]
    public bool Description { get; init; }
}