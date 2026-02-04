using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Proxxi.Core.ProxyWriters.Converters;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public sealed class JsonProxyWriter(Stream stream, bool writeIndented = false) : IBatchProxyWriter, IStreamProxyWriter
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        Converters = { new ProtocolsJsonConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = writeIndented
    };

    public async Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        await JsonSerializer.SerializeAsync(stream, proxies, _options, cancellationToken);

    public async Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default) =>
        await JsonSerializer.SerializeAsync(stream, proxies, _options, cancellationToken);
}