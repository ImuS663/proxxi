using Microsoft.Extensions.Options;

using Moq;

using Proxxi.Cli.Commands.Plugin.PluginParameter;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginParameter;

[TestFixture(TestOf = typeof(PluginParameterCommand))]
public class PluginParameterCommandTests
{
    private TestConsole _console;
    private IOptions<ProxxiPathsOptions> _options;
    private Mock<IPluginConfigProvider> _mockPluginConfigProvider;
    private Mock<IPluginLoader> _mockPluginLoader;
    private PluginConfig[] _configs;
    private PluginDescriptor[] _descriptors;

    private string PluginsDir => Path.Combine(_options.Value.PluginsDir);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _options = Options.Create(new ProxxiPathsOptions { ProxxiDir = Path.Combine(dir) });
    }

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();
        _console.Profile.Width = 200;

        _configs = PluginTestData.GetConfigs().ToArray();
        _descriptors = PluginTestData.GetDescriptors(PluginsDir).ToArray();

        _mockPluginConfigProvider = new Mock<IPluginConfigProvider>();

        _mockPluginConfigProvider.Setup(x => x.Get("test.plugin1")).Returns(_configs[0]);
        _mockPluginConfigProvider.Setup(x => x.Get("test.plugin2")).Returns(_configs[1]);
        _mockPluginConfigProvider.Setup(x => x.Get("test.plugin3")).Returns(_configs[2]);
        _mockPluginConfigProvider.Setup(x => x.Get("test.plugin4")).Returns((PluginConfig?)null);

        _mockPluginConfigProvider.Setup(x => x.AliasExists("p-alias", "test.plugin1")).Returns(true);

        _mockPluginLoader = new Mock<IPluginLoader>();

        var fullPath1 = Path.Combine(PluginsDir, "pack1/test.plugin.dll");
        var fullPath2 = Path.Combine(PluginsDir, "pack2/test.plugin.dll");

        _mockPluginLoader.Setup(x => x.LoadPlugin(fullPath1, "test.plugin1")).Returns(_descriptors[0]);
        _mockPluginLoader.Setup(x => x.LoadPlugin(fullPath2, "test.plugin2")).Returns((PluginDescriptor?)null);
        _mockPluginLoader.Setup(x => x.LoadPlugin(fullPath1, "test.plugin3")).Returns(_descriptors[2]);
    }

    [TearDown]
    public void TearDown()
    {
        _console.Dispose();

        _mockPluginConfigProvider.Reset();
    }

    [Test]
    public void Execute_WhenPluginIsNotInstalled_ThrowsInvalidOperationException()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings { Id = "test.plugin4", Name = "test", Value = "test" };

        var extension =
            Assert.Throws<InvalidOperationException>(() => command.Execute(null!, settings, CancellationToken.None));

        Assert.That(extension.Message, Is.EqualTo("Plugin 'test.plugin4' is not installed."));
    }

    [Test]
    public void Execute_WhenPluginIsNotLoaded_ThrowsInvalidOperationException()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings { Id = "test.plugin2", Name = "test", Value = "test" };

        var extension =
            Assert.Throws<InvalidOperationException>(() => command.Execute(null!, settings, CancellationToken.None));

        Assert.That(extension.Message, Is.EqualTo("Plugin 'test.plugin2' is not loaded."));
    }

    [Test]
    public void Execute_WhenPluginIsNotSupportParameter_ThrowsInvalidOperationException()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings { Id = "test.plugin3", Name = "test", Value = "test" };

        var extension =
            Assert.Throws<InvalidOperationException>(() => command.Execute(null!, settings, CancellationToken.None));

        Assert.That(extension.Message, Is.EqualTo("Plugin does not support parameter 'test'."));
    }

    [Test]
    public void Execute_WhenPluginIsNotSupportParameterAndForceFlag_ReturnsZeroAndSetsParameter()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings
        {
            Id = "test.plugin3",
            Name = "test-param-name",
            Value = "test-param-value",
            Force = true
        };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);

            Assert.That(_configs[2].Parameters,
                Does.Contain(new KeyValuePair<string, string>("test-param-name", "test-param-value")));

            _mockPluginConfigProvider.Verify(x => x.Upsert(It.IsAny<PluginConfig>()), Times.Once);
            _mockPluginConfigProvider.Verify(x => x.Save(), Times.Once);
        }
    }

    [Test]
    public void Execute_WhenPluginIsSupportParameter_ReturnsZeroAndSetsParameter()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings { Id = "test.plugin3", Name = "page", Value = "4" };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_configs[2].Parameters, Does.Contain(new KeyValuePair<string, string>("page", "4")));

            _mockPluginConfigProvider.Verify(x => x.Upsert(It.IsAny<PluginConfig>()), Times.Once);
            _mockPluginConfigProvider.Verify(x => x.Save(), Times.Once);
        }
    }

    [Test]
    public void Execute_WhenPluginIsSupportParameterAndRemoveFlag_ReturnsZeroAndSetsParameter()
    {
        var command = new PluginParameterCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginParameterCommandSettings { Id = "test.plugin3", Name = "key", Remove = true };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_configs[2].Parameters, Does.Not.ContainKey("key"));

            _mockPluginConfigProvider.Verify(x => x.Upsert(It.IsAny<PluginConfig>()), Times.Once);
            _mockPluginConfigProvider.Verify(x => x.Save(), Times.Once);
        }
    }
}