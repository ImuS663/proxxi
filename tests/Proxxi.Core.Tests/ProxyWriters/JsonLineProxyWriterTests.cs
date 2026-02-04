using Proxxi.Core.ProxyWriters;
using Proxxi.Core.Tests.TestData;

namespace Proxxi.Core.Tests.ProxyWriters;

[TestFixture(TestOf = typeof(JsonLineProxyWriter))]
public class JsonLineProxyWriterTests
{
    private const string ExpectedJsonLines = """
                                             {"host":"44.44.44.44","port":8080,"username":"user","password":"pass","protocols":["http"]}
                                             {"host":"88.88.88.88","port":8080,"username":"user","password":"pass","protocols":["https"]}
                                             {"host":"11.11.11.11","port":6080,"protocols":["socks4","socks5"]}
                                             {"host":"22.22.22.22","port":8080,"username":"user","password":"pass","protocols":[]}

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
    public async Task WriteAsync_IEnumerableProxies_WritesExpectedJsonLines()
    {
        var writer = new JsonLineProxyWriter(_stream);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedJsonLines));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WritesExpectedJsonLines()
    {
        var writer = new JsonLineProxyWriter(_stream);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedJsonLines));
    }
}