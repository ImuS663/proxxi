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
)
    : AsyncCommand<FetchCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override async Task<int> ExecuteAsync(CommandContext context, FetchCommandSettings settings,
        CancellationToken ct)
    {
        Stream stream;
        OutputFormat format;

        var protocols = settings.Protocols ?? Protocols.None;

        if (settings.Output != null)
        {
            stream = File.OpenWrite(settings.Output);

            if (!Enum.TryParse(Path.GetExtension(settings.Output).TrimStart('.'), true, out format))
                format = settings.Format;
        }
        else
        {
            stream = Console.OpenStandardOutput();
            format = settings.Format;
        }

        try
        {
            (IBatchProxySource? batchProxySource, IStreamProxySource? streamProxySource) =
                await GetPluginInstance(settings.Id, ct);

            var writer = CreateProxyWriter(stream, format, settings.Pretty);

            if (settings.Stream)
            {
                if (writer is not IStreamProxyWriter streamProxyWriter)
                    throw new InvalidOperationException($"{format} output does not support stream mode.");

                await FetchAndWriteProxiesAsync(streamProxySource, streamProxyWriter, protocols, ct);
            }
            else
            {
                if (writer is not IBatchProxyWriter batchProxyWriter)
                    throw new InvalidOperationException($"{format} output does not support stream mode.");

                await FetchAndWriteProxiesAsync(batchProxySource, batchProxyWriter, protocols, ct);
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            console.MarkupLine("[yellow]Info:[/] Operation canceled.");
            return 130;
        }
        finally
        {
            if (settings.Output != null)
                await stream.DisposeAsync();
        }
    }

    private async Task<(IBatchProxySource?, IStreamProxySource?)> GetPluginInstance(string id, CancellationToken ct)
    {
        var pluginConfig = configProvider.Get(id);

        if (pluginConfig == null)
            throw new InvalidOperationException($"Plugin '{id}' is not installed.");

        if (!pluginConfig.Enabled)
            throw new InvalidOperationException($"Plugin '{id}' is disabled.");

        var fullPath = Path.Combine(_pathOptions.PluginsDir, pluginConfig.Path);

        var plugin = pluginLoader.LoadPlugin(fullPath, id);

        if (plugin == null)
            throw new InvalidOperationException($"Plugin '{id}' is not loaded.");

        return await plugin.CreateAsync(pluginConfig.Parameters.ToDictionary(), ct);
    }

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