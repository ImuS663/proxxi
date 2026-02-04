using Proxxi.Core.ProxyWriters;
using Proxxi.Core.Tests.TestData;

namespace Proxxi.Core.Tests.ProxyWriters;

[TestFixture(TestOf = typeof(XmlProxyWriter))]
public class XmlProxyWriterTests
{
    private const string ExpectedXmlWithWriteIndented = """
                                                        <?xml version="1.0" encoding="utf-8"?>
                                                        <proxies>
                                                          <proxy host="44.44.44.44" port="8080">
                                                            <username>user</username>
                                                            <password>pass</password>
                                                            <protocols>
                                                              <protocol name="http" />
                                                            </protocols>
                                                          </proxy>
                                                          <proxy host="88.88.88.88" port="8080">
                                                            <username>user</username>
                                                            <password>pass</password>
                                                            <protocols>
                                                              <protocol name="https" />
                                                            </protocols>
                                                          </proxy>
                                                          <proxy host="11.11.11.11" port="6080">
                                                            <protocols>
                                                              <protocol name="socks4" />
                                                              <protocol name="socks5" />
                                                            </protocols>
                                                          </proxy>
                                                          <proxy host="22.22.22.22" port="8080">
                                                            <username>user</username>
                                                            <password>pass</password>
                                                            <protocols />
                                                          </proxy>
                                                        </proxies>
                                                        """;

    private const string ExpectedXmlWithoutWriteIndented =
        """<?xml version="1.0" encoding="utf-8"?>""" +
        """<proxies><proxy host="44.44.44.44" port="8080"><username>user</username><password>pass</password><protocols><protocol name="http" /></protocols></proxy>""" +
        """<proxy host="88.88.88.88" port="8080"><username>user</username><password>pass</password><protocols><protocol name="https" /></protocols></proxy>""" +
        """<proxy host="11.11.11.11" port="6080"><protocols><protocol name="socks4" /><protocol name="socks5" /></protocols></proxy>""" +
        """<proxy host="22.22.22.22" port="8080"><username>user</username><password>pass</password><protocols /></proxy></proxies>""";

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
    public async Task WriteAsync_IEnumerableProxies_WithWriteIndented_WritesExpectedXml()
    {
        var writer = new XmlProxyWriter(_stream, true);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedXmlWithWriteIndented));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithWriteIndented_WritesExpectedXml()
    {
        var writer = new XmlProxyWriter(_stream, true);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedXmlWithWriteIndented));
    }

    [Test]
    public async Task WriteAsync_IEnumerableProxies_WithoutWriteIndented_WritesExpectedXml()
    {
        var writer = new XmlProxyWriter(_stream, false);
        var proxies = Proxies.GetEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedXmlWithoutWriteIndented));
    }

    [Test]
    public async Task WriteAsync_IAsyncEnumerableProxies_WithoutWriteIndented_WritesExpectedXml()
    {
        var writer = new XmlProxyWriter(_stream, false);
        var proxies = Proxies.GetAsyncEnumerable();

        await writer.WriteAsync(proxies);

        _stream.Position = 0;
        var result = await new StreamReader(_stream).ReadToEndAsync();

        Assert.That(result, Is.EqualTo(ExpectedXmlWithoutWriteIndented));
    }
}