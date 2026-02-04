using System.Xml;

using Proxxi.Core.Extensions;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public sealed class XmlProxyWriter(Stream stream, bool writeIndented = false) : IBatchProxyWriter, IStreamProxyWriter
{
    private readonly XmlWriterSettings _settings = new() { Async = true, Indent = writeIndented };

    public async Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        await WriteAsync(proxies.ToAsyncEnumerable(), cancellationToken);

    public async Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default)
    {
        await using var writer = XmlWriter.Create(stream, _settings);

        await writer.WriteStartDocumentAsync();

        await writer.WriteStartElementAsync(null, "proxies", null);

        await foreach (var proxy in proxies.WithCancellation(cancellationToken))
        {
            await writer.WriteStartElementAsync(null, "proxy", null);
            await writer.WriteAttributeStringAsync(null, "host", null, proxy.Host);
            await writer.WriteAttributeStringAsync(null, "port", null, proxy.Port.ToString());

            if (proxy.Username != null)
                await writer.WriteElementStringAsync(null, "username", null, proxy.Username);

            if (proxy.Password != null)
                await writer.WriteElementStringAsync(null, "password", null, proxy.Password);

            await writer.WriteStartElementAsync(null, "protocols", null);

            foreach (var protocol in proxy.Protocols.ToStrings())
            {
                await writer.WriteStartElementAsync(null, "protocol", null);
                await writer.WriteAttributeStringAsync(null, "name", null, protocol);
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();

            await writer.WriteEndElementAsync();
        }

        await writer.WriteEndElementAsync();

        await writer.WriteEndDocumentAsync();
    }
}