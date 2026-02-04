using Proxxi.Core.ProxyWriters;
using Proxxi.Core.Tests.TestData;

namespace Proxxi.Core.Tests.ProxyWriters;

[TestFixture(TestOf = typeof(PsvProxyWriter))]
public class PsvProxyWriterTests
{
    private const string ExpectedPsvWithHeader = """
                                                 host|port|username|password|protocols
                                                 44.44.44.44|8080|user|pass|http
                                                 88.88.88.88|8080|user|pass|https
                                                 11.11.11.11|6080|||socks4,socks5
                                                 22.22.22.22|8080|user|pass|

                                                 """;

    private const string ExpectedPsvWithoutHeader = """
                                                    44.44.44.44|8080|user|pass|http
                                                    88.88.88.88|8080|user|pass|https
                                                    11.11.11.11|6080|||socks4,socks5
                                                    22.22.22.22|8080|user|pass|

                                                    """;

    private MemoryStream _stream;

    [SetUp]
    public void SetUp()
    {
        _stream = new MemoryStream();
    }

    [TearDown]
    public void TearDown()
    {
        _stream.Dispose();
    }

    [Test]
    public async Task WriteAsync_IEnumerableProxies_WithHeader_WritesExpectedPsv()
    {
        var writer = new PsvProxyWriter(_stream, true);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPsvWithHeader));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithHeader_WritesExpectedPsv()
    {
        var writer = new PsvProxyWriter(_stream, true);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPsvWithHeader));
    }

    [Test]
    public async Task WriteAsync_IEnumerableProxies_WithoutHeader_WritesExpectedPsv()
    {
        var writer = new PsvProxyWriter(_stream, false);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPsvWithoutHeader));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithoutHeader_WritesExpectedPsv()
    {
        var writer = new PsvProxyWriter(_stream, false);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPsvWithoutHeader));
    }
}