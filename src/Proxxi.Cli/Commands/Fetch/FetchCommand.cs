using Proxxi.Cli.Services;
using Proxxi.Core.Models;
using Proxxi.Core.ProxyWriters;
using Proxxi.Plugin.Sdk.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Fetch;

public sealed class FetchCommand(IAnsiConsole console, ProxyFetcher fetcher) : AsyncCommand<FetchCommandSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, FetchCommandSettings settings,
        CancellationToken ct)
    {
        var protocols = settings.Protocols ?? Protocols.None;

        try
        {
            await using var stream = CreateOutputStreamAsync(settings);
            var format = ResolveFormat(settings);

            var writer = CreateProxyWriter(stream, format, settings.Pretty);

            if (settings.Stream)
            {
                var proxies = ApplyStreamFilter(await fetcher.FetchStreamAsync(settings.Id, ct));

                await writer.WriteAsync(proxies, ct);
            }
            else
            {
                var proxies = ApplyBatchFilter(await fetcher.FetchBatchAsync(settings.Id, ct));

                await writer.WriteAsync(proxies, ct);
            }

            console.MarkupLine("[green]✓[/] Fetching complete.");

            return 0;

            IAsyncEnumerable<Proxy> ApplyStreamFilter(IAsyncEnumerable<Proxy> src) =>
                protocols != Protocols.None ? src.Where(p => (p.Protocols & protocols) != Protocols.None) : src;

            IEnumerable<Proxy> ApplyBatchFilter(IEnumerable<Proxy> src) =>
                protocols != Protocols.None ? src.Where(p => (p.Protocols & protocols) != Protocols.None) : src;
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine("[yellow]Info:[/] Operation canceled.");
            return 130;
        }
    }

    private Stream CreateOutputStreamAsync(FetchCommandSettings settings)
    {
        if (settings.Output == null)
            return Console.OpenStandardOutput();

        if (File.Exists(settings.Output) && !settings.Yes && !console.Prompt(ConfirmOverwritePrompt(settings)))
            throw new OperationCanceledException();

        return File.Create(settings.Output);
    }

    private static OutputFormat ResolveFormat(FetchCommandSettings settings)
    {
        if (settings.Output == null)
            return settings.Format;

        return Enum.TryParse(Path.GetExtension(settings.Output).TrimStart('.'), true, out OutputFormat format)
            ? format
            : settings.Format;
    }

    private static ConfirmationPrompt ConfirmOverwritePrompt(FetchCommandSettings settings) =>
        new($"File '{settings.Output}' already exists. Overwrite?") { DefaultValue = false };

    private static IProxyWriter CreateProxyWriter(Stream stream, OutputFormat format, bool isPretty) =>
        format switch
        {
            OutputFormat.Plain => new PlainProxyWriter(stream),
            OutputFormat.Url => new UrlProxyWriter(stream),
            OutputFormat.Json => new JsonProxyWriter(stream, isPretty),
            OutputFormat.Jsonl => new JsonLineProxyWriter(stream),
            OutputFormat.Xml => new XmlProxyWriter(stream, isPretty),
            OutputFormat.Csv => new CsvProxyWriter(stream, isPretty),
            OutputFormat.Psv => new PsvProxyWriter(stream, isPretty),
            OutputFormat.Tsv => new TsvProxyWriter(stream, isPretty),
            _ => new PlainProxyWriter(stream)
        };
}