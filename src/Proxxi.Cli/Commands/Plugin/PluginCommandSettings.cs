using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Plugin;

public abstract class PluginCommandSettings : CommandSettings
{
    [CommandArgument(0, "<ID>")]
    [Description("The plugin ID to plugin manage")]
    public required string Id { get; init; }
}