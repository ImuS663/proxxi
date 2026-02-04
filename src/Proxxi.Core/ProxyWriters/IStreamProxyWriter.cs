using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public interface IStreamProxyWriter
{
    public Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default);
}