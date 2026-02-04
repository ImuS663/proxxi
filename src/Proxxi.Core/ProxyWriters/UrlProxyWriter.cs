using System.Text;

using Proxxi.Core.Extensions;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public class UrlProxyWriter(Stream stream) : IBatchProxyWriter, IStreamProxyWriter
{
    private static readonly Encoding Encoding = new UTF8Encoding(false);

    public Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        WriteAsync(proxies.ToAsyncEnumerable(), cancellationToken);

    public async Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(stream, Encoding, leaveOpen: true);

        await foreach (var proxy in proxies.WithCancellation(cancellationToken))
        {
            foreach (var protocol in proxy.Protocols.ToStrings())
            {
                var auth = proxy.Username != null && proxy.Password != null
                    ? $"{proxy.Username}:{proxy.Password}@"
                    : string.Empty;

                await writer.WriteLineAsync($"{protocol}://{auth}{proxy.Host}:{proxy.Port}");
            }
        }
    }
}