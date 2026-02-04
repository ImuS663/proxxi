using System.Text;

using Proxxi.Core.Extensions;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public abstract class DsvProxyWriter(Stream stream, bool writeHeader, char separator)
    : IBatchProxyWriter, IStreamProxyWriter
{
    private static readonly Encoding Encoding = new UTF8Encoding(false);
    private static readonly string[] Headers = ["host", "port", "username", "password", "protocols"];

    public async Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        await WriteAsync(proxies.ToAsyncEnumerable(), cancellationToken);

    public async Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(stream, Encoding, leaveOpen: true);

        if (writeHeader)
            await writer.WriteLineAsync(string.Join(separator, Headers));

        await foreach (var proxy in proxies.WithCancellation(cancellationToken))
            await writer.WriteLineAsync(MakeLine(proxy));
    }

    private string MakeLine(Proxy proxy)
    {
        var protocols = string.Join(",", proxy.Protocols.ToStrings());

        return string.Join(separator,
            Escape(proxy.Host),
            proxy.Port.ToString(),
            Escape(proxy.Username),
            Escape(proxy.Password),
            Escape(protocols)
        );
    }

    private string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        var mustQuote =
            value.Contains(separator) ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        return mustQuote ? string.Concat("\"", value.Replace("\"", "\"\""), "\"") : value;
    }
}