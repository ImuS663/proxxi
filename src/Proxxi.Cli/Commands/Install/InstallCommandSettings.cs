using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Install;

public sealed class InstallCommandSettings : CommandSettings
{
    [CommandArgument(0, "<PATH>")]
    [Description("The path to the plugin package (.pxp file)")]
    public required string Path { get; init; }

    [CommandOption("-u|--update"), DefaultValue(false)]
    [Description("Update the plugin if it already exists")]
    public bool Update { get; init; }
}