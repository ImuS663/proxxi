using Proxxi.Core.ProxyWriters;
using Proxxi.Core.Tests.TestData;

namespace Proxxi.Core.Tests.ProxyWriters;

[TestFixture(TestOf = typeof(PlainProxyWriter))]
public class PlainProxyWriterTests
{
    private const string ExpectedPlainText = """
                                        user:pass@44.44.44.44:8080
                                        user:pass@88.88.88.88:8080
                                        11.11.11.11:6080
                                        user:pass@22.22.22.22:8080

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
    public async Task WriteAsync_IEnumerableProxies_WritesExpectedPlainText()
    {
        var writer = new PlainProxyWriter(_stream);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPlainText));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithHeader_WritesExpectedPlainText()
    {
        var writer = new PlainProxyWriter(_stream);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedPlainText));
    }
}