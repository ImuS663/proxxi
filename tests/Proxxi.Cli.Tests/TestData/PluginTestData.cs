using Proxxi.Core.Models;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Sdk.ProxySources;

namespace Proxxi.Cli.Tests.TestData;

internal static class PluginTestData
{
    private static readonly PluginConfig[] Configs =
    [
        new()
        {
            Id = "test.plugin1",
            Path = "pack1/test.plugin.dll",
            Version = "1.0.0",
            Enabled = false
        },
        new()
        {
            Id = "test.plugin2",
            Path = "pack2/test.plugin.dll",
            Version = "1.2.0",
            Enabled = true
        },
        new()
        {
            Id = "test.plugin3",
            Path = "pack1/test.plugin.dll",
            Version = "1.6.0",
            Alias = "p-alias",
            Parameters = { { "key", "value" } }
        }
    ];

    private static readonly PluginDescriptor[] Descriptors =
    [
        new(
            Id: "test.plugin1",
            Name: "Test Plugin 1",
            Description: "Test Plugin 1 Desc",
            Version: "1.0.0",
            Path: "pack1/test.plugin.dll",
            HideBatch: true,
            HideStream: false,
            Parameters: [],
            ProxySourceType: typeof(IStreamProxySource)
        ),
        new(
            Id: "test.plugin2",
            Name: "Test Plugin 2",
            Description: "Test Plugin 2 Desc",
            Version: "1.2.0",
            Path: "pack2/test.plugin.dll",
            HideBatch: true,
            HideStream: true,
            Parameters: [],
            ProxySourceType: typeof(IProxySource)
        ),
        new(
            Id: "test.plugin3",
            Name: "Test Plugin 3",
            Description: "Test Plugin 3 Desc",
            Version: "1.6.0",
            Path: "pack1/test.plugin.dll",
            HideBatch: false,
            HideStream: true,
            Parameters:
            [
                new PluginParameter("key", "Key for test plugin", true),
                new PluginParameter("page", "Page for test plugin")
            ],
            ProxySourceType: typeof(IProxySource))
    ];

    public static IEnumerable<PluginConfig> GetConfigs() =>
        Configs.Select(config => new PluginConfig
        {
            Id = config.Id,
            Path = config.Path,
            Version = config.Version,
            Enabled = config.Enabled,
            Alias = config.Alias,
            Parameters = config.Parameters.ToDictionary(x => x.Key, x => x.Value)
        });

    public static IEnumerable<PluginDescriptor> GetDescriptors(string path) =>
        Descriptors.Select(descriptor => descriptor with { Path = Path.Combine(path, descriptor.Path) });
}