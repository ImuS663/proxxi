using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public interface IProxyWriter
{
    public Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default);
    public Task WriteAsync(IAsyncEnumerable<Proxy> proxies, CancellationToken cancellationToken = default);
}