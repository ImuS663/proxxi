using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Core.ProxyWriters;
using Proxxi.Plugin.Loader.Extensions;
using Proxxi.Plugin.Loader.PluginLoaders;
using Proxxi.Plugin.Sdk.Models;
using Proxxi.Plugin.Sdk.ProxySources;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Fetch;

public sealed class FetchCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
) : AsyncCommand<FetchCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override async Task<int> ExecuteAsync(CommandContext context, FetchCommandSettings settings,
        CancellationToken ct)
    {
        var protocols = settings.Protocols ?? Protocols.None;

        try
        {
            await using var stream = CreateOutputStreamAsync(settings, ct);
            var format = ResolveFormat(settings);

            (IBatchProxySource? batchProxySource, IStreamProxySource? streamProxySource) =
                await GetPluginInstance(settings.Id, ct);

            var writer = CreateProxyWriter(stream, format, settings.Pretty);

            switch (settings.Stream, writer)
            {
                case (true, IStreamProxyWriter streamProxyWriter):
                    await FetchAndWriteProxiesAsync(streamProxySource, streamProxyWriter, protocols, ct);
                    break;
                case (false, IBatchProxyWriter batchProxyWriter):
                    await FetchAndWriteProxiesAsync(batchProxySource, batchProxyWriter, protocols, ct);
                    break;
                case (true, _):
                    throw new InvalidOperationException($"{format} output does not support stream mode.");
                default:
                    throw new InvalidOperationException($"{format} output does not support batch mode.");
            }

            console.MarkupLine("[green]✓[/] Fetching complete.");

            return 0;
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine("[yellow]Info:[/] Operation canceled.");
            return 130;
        }
    }

    private Stream CreateOutputStreamAsync(FetchCommandSettings settings, CancellationToken ct)
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

    private async Task<(IBatchProxySource?, IStreamProxySource?)> GetPluginInstance(string id, CancellationToken ct)
    {
        var pluginConfig = configProvider.Get(id);

        if (pluginConfig == null)
            throw new InvalidOperationException($"Plugin '{id}' is not installed.");

        if (!pluginConfig.Enabled)
            throw new InvalidOperationException($"Plugin '{id}' is disabled.");

        var fullPath = Path.Combine(_pathOptions.PluginsDir, pluginConfig.Path);

        var plugin = pluginLoader.LoadPlugin(fullPath, pluginConfig.Id);

        if (plugin == null)
            throw new InvalidOperationException($"Plugin '{id}' is not loaded.");

        return await plugin.CreateAsync(pluginConfig.Parameters.ToDictionary(), ct);
    }

    private static ConfirmationPrompt ConfirmOverwritePrompt(FetchCommandSettings settings) =>
        new($"File '{settings.Output}' already exists. Overwrite?") { DefaultValue = false };

    private static async Task FetchAndWriteProxiesAsync(IBatchProxySource? batchProxySource,
        IBatchProxyWriter writer, Protocols protocols, CancellationToken ct)
    {
        if (batchProxySource == null)
            throw new InvalidOperationException("The plugin does not support batch mode.");

        var proxies = await batchProxySource.FetchAsync(ct);

        if (protocols != Protocols.None)
            proxies = proxies.Where(p => (p.Protocols & protocols) != Protocols.None);

        await writer.WriteAsync(proxies, ct);
    }

    private static async Task FetchAndWriteProxiesAsync(IStreamProxySource? streamProxySource,
        IStreamProxyWriter writer, Protocols protocols, CancellationToken ct)
    {
        if (streamProxySource == null)
            throw new InvalidOperationException("The plugin does not support stream mode.");

        var proxies = streamProxySource.FetchAsync(ct);

        if (protocols != Protocols.None)
            proxies = proxies.Where(p => (p.Protocols & protocols) != Protocols.None);

        await writer.WriteAsync(proxies, ct);
    }

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