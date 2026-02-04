using Proxxi.Core.Extensions;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.Tests.Extensions;

[TestFixture(TestOf = typeof(ProtocolsExtension))]
public class ProtocolsExtensionTests
{
    private static readonly string[] ExpectedProtocols = ["http", "https"];
    private static readonly string[] ExpectedSingleProtocol = ["socks5"];

    [Test]
    public void ToStrings_WithHttpAndHttps_ReturnsExpectedValues()
    {
        const Protocols protocols = Protocols.Http | Protocols.Https;

        var result = protocols.ToStrings();

        Assert.That(result, Is.EquivalentTo(ExpectedProtocols));
    }

    [Test]
    public void ToStrings_WithSingleProtocol_ReturnsExpectedValue()
    {
        const Protocols protocols = Protocols.Socks5;

        var result = protocols.ToStrings();

        Assert.That(result, Is.EquivalentTo(ExpectedSingleProtocol));
    }

    [Test]
    public void ToStrings_WithNoneProtocol_ReturnsEmpty()
    {
        const Protocols protocols = Protocols.None;

        var result = protocols.ToStrings();

        Assert.That(result, Is.Empty);
    }
}