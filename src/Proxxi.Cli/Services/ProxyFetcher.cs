using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Extensions;
using Proxxi.Plugin.Loader.PluginLoaders;
using Proxxi.Plugin.Sdk.Models;
using Proxxi.Plugin.Sdk.ProxySources;

namespace Proxxi.Cli.Services;

public class ProxyFetcher(
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
)
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public async Task<IEnumerable<Proxy>> FetchBatchAsync(string id, CancellationToken ct)
    {
        var pluginConfig = GetPluginConfig(id);
        var pluginInstance = await GetPluginInstance(pluginConfig, t => t.Batch, ct);

        return await pluginInstance.FetchAsync(ct);
    }

    public async Task<IAsyncEnumerable<Proxy>> FetchStreamAsync(string id, CancellationToken ct)
    {
        var pluginConfig = GetPluginConfig(id);
        var pluginInstance = await GetPluginInstance(pluginConfig, t => t.Stream, ct);

        return pluginInstance.FetchAsync(ct);
    }

    private PluginConfig GetPluginConfig(string id)
    {
        var pluginConfig = configProvider.Get(id);

        if (pluginConfig == null)
            throw new InvalidOperationException($"Plugin '{id}' is not installed.");

        if (!pluginConfig.Enabled)
            throw new InvalidOperationException($"Plugin '{id}' is disabled.");

        return pluginConfig;
    }

    private async Task<TPlugin> GetPluginInstance<TPlugin>(PluginConfig pluginConfig,
        Func<(IBatchProxySource? Batch, IStreamProxySource? Stream), TPlugin?> selector, CancellationToken ct)
        where TPlugin : IProxySource
    {
        var fullPath = Path.Combine(_pathOptions.PluginsDir, pluginConfig.Path);

        var plugin = pluginLoader.LoadPlugin(fullPath, pluginConfig.Id);

        if (plugin == null)
            throw new InvalidOperationException($"Plugin '{pluginConfig.Id}' is not loaded.");

        var instance = selector(await plugin.CreateAsync(pluginConfig.Parameters.ToDictionary(), ct));

        return instance ?? throw new InvalidOperationException($"Plugin '{plugin.Id}' does not support this mode.");
    }
}