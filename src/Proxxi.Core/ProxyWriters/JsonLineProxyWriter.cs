using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Proxxi.Core.ProxyWriters.Converters;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public sealed class JsonLineProxyWriter(Stream stream) : IBatchProxyWriter, IStreamProxyWriter
{
    private static readonly Encoding Encoding = new UTF8Encoding(false);

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        Converters = { new ProtocolsJsonConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };


    public async Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        await WriteAsync(proxies.ToAsyncEnumerable(), cancellationToken);

    public async Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(stream, Encoding, leaveOpen: true);

        await foreach (var proxy in proxies.WithCancellation(cancellationToken))
            await writer.WriteLineAsync(JsonSerializer.Serialize(proxy, _options));
    }
}