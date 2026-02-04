using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Sdk.ProxySources;

namespace Proxxi.Plugin.Loader.Extensions;

public static class PluginDescriptorExtension
{
    public static async Task<(IBatchProxySource?, IStreamProxySource?)> CreateAsync(this PluginDescriptor descriptor,
        IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        if (Activator.CreateInstance(descriptor.ProxySourceType, nonPublic: false) is not IProxySource proxySource)
            throw new InvalidOperationException($"Failed to create an instance of {descriptor.ProxySourceType}.");

        await proxySource.InitializeAsync(parameters, cancellationToken);

        var batchProxySource = !descriptor.HideBatch ? proxySource as IBatchProxySource : null;
        var streamProxySource = !descriptor.HideStream ? proxySource as IStreamProxySource : null;

        return (batchProxySource, streamProxySource);
    }
}