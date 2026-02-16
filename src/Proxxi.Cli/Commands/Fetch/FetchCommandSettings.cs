using System.ComponentModel;

using Proxxi.Core.Models;
using Proxxi.Plugin.Sdk.Models;

using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Fetch;

public class FetchCommandSettings : CommandSettings
{
    [CommandArgument(0, "<ID>")]
    [Description("The plugin ID or alias to fetch proxies from")]
    public required string Id { get; init; }

    [CommandOption("-o|--output <PATH>")]
    [Description("Write the fetched proxies to the specified file (defaults to stdout)")]
    public string? Output { get; init; }

    [CommandOption("-f|--format <FORMAT>"), DefaultValue(OutputFormat.Plain)]
    [Description("Output format: plain, url, json, jsonl, xml, csv, psv, tsv")]
    public OutputFormat Format { get; init; }

    [CommandOption("-s|--stream"), DefaultValue(false)]
    [Description("Fetch proxies using streaming mode instead of batch mode")]
    public bool Stream { get; init; }

    [CommandOption("-p|--protocols <PROTOCOLS>"), DefaultValue(Proxxi.Plugin.Sdk.Models.Protocols.None)]
    [Description("Filter proxies by protocol: http, https, socks4, socks5")]
    public Protocols? Protocols { get; init; }

    [CommandOption("--pretty")]
    [Description("Write human-readable, pretty-formatted output")]
    public bool Pretty { get; init; }
}