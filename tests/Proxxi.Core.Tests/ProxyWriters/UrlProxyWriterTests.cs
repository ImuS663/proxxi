using Proxxi.Core.ProxyWriters;
using Proxxi.Core.Tests.TestData;

namespace Proxxi.Core.Tests.ProxyWriters;

[TestFixture(TestOf = typeof(UrlProxyWriter))]
public class UrlProxyWriterTests
{
    private const string ExpectedUrl = """
                                       http://user:pass@44.44.44.44:8080
                                       https://user:pass@88.88.88.88:8080
                                       socks4://11.11.11.11:6080
                                       socks5://11.11.11.11:6080

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
    public async Task WriteAsync_IEnumerableProxies_WritesExpectedUrl()
    {
        var writer = new UrlProxyWriter(_stream);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedUrl));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithHeader_WritesExpectedUrl()
    {
        var writer = new UrlProxyWriter(_stream);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedUrl));
    }
}