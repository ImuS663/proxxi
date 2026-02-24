using System.ComponentModel;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Pack.Remove;

public sealed class RemoveCommandSettings : CommandSettings
{
    [CommandArgument(0, "<ID>")]
    [Description("The plugin ID to remove")]
    public required string Id { get; init; }

    [CommandOption("-y|--yes"), DefaultValue(false)]
    [Description("Skip confirmation prompt")]
    public bool Yes { get; init; }
}