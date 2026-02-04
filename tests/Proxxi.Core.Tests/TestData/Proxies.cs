using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.Tests.TestData;

internal sealed class Proxies
{
    private static readonly Proxy[] ProxyArray =
    [
        new("44.44.44.44", 8080, "user", "pass", Protocols.Http),
        new("88.88.88.88", 8080, "user", "pass", Protocols.Https),
        new("11.11.11.11", 6080, null, null, Protocols.Socks4 | Protocols.Socks5),
        new("22.22.22.22", 8080, "user", "pass", Protocols.None)
    ];

    public static async IAsyncEnumerable<Proxy> GetAsyncEnumerable()
    {
        foreach (var proxy in ProxyArray)
        {
            yield return proxy;
        }

        await Task.Yield();
    }

    public static IEnumerable<Proxy> GetEnumerable() => ProxyArray;
}