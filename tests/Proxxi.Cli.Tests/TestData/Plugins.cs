using Proxxi.Core.Models;

namespace Proxxi.Cli.Tests.TestData;

internal static class Plugins
{
    private static readonly PluginConfig[] PluginArray =
    [
        new() { Id = "test.plugin1", Path = "pack1/test.plugin.dll", Version = "1.0.0", Enabled = false },
        new() { Id = "test.plugin2", Path = "pack2/test.plugin.dll", Version = "1.2.0", Enabled = true },
        new() { Id = "test.plugin3", Path = "pack1/test.plugin.dll", Version = "1.6.0", Alias = "p-alias" }
    ];

    public static IEnumerable<PluginConfig> GetConfigs()
    {
        return PluginArray.Select(pluginConfig => new PluginConfig
        {
            Id = pluginConfig.Id,
            Path = pluginConfig.Path,
            Version = pluginConfig.Version,
            Enabled = pluginConfig.Enabled,
            Alias = pluginConfig.Alias
        });
    }
}