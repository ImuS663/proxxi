using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters;

public interface IBatchProxyWriter : IProxyWriter
{
    public Task WriteAsync(IEnumerable<Proxy> proxies, CancellationToken cancellationToken = default);
}