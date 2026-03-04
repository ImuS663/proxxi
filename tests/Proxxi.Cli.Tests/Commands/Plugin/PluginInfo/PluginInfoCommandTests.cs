using Microsoft.Extensions.Options;

using Moq;

using Proxxi.Cli.Commands.Plugin.PluginInfo;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginInfo;

[TestFixture(TestOf = typeof(PluginInfoCommand))]
public class PluginInfoCommandTests
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
        var command = new PluginInfoCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginInfoCommand.PluginInfoCommandSettings { Id = "test.plugin4" };

        var extension =
            Assert.Throws<InvalidOperationException>(() => command.Execute(null!, settings, CancellationToken.None));

        Assert.That(extension.Message, Is.EqualTo("Plugin 'test.plugin4' is not installed."));
    }

    [Test]
    public void Execute_WhenPluginIsNotLoaded_ThrowsInvalidOperationException()
    {
        var command = new PluginInfoCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginInfoCommand.PluginInfoCommandSettings { Id = "test.plugin2" };

        var extension =
            Assert.Throws<InvalidOperationException>(() => command.Execute(null!, settings, CancellationToken.None));

        Assert.That(extension.Message, Is.EqualTo("Plugin 'test.plugin2' is not loaded."));
    }

    [Test]
    public void Execute_WhenPluginIsInstalled_PrintsPluginInfo()
    {
        var command = new PluginInfoCommand(_console, _mockPluginConfigProvider.Object, _mockPluginLoader.Object,
            _options);

        var settings = new PluginInfoCommand.PluginInfoCommandSettings { Id = "test.plugin1" };

        var result = command.Execute(null!, settings, CancellationToken.None);

        var lines = _console.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(lines[0], Does.Contain("Test Plugin 1"));
            Assert.That(lines[1], Does.Contain("Id").And.Contain(_descriptors[0].Id).And.Contain("disabled"));
            Assert.That(lines[2], Does.Contain("Alias").And.Contain("<none>"));
            Assert.That(lines[3], Does.Contain("Description").And.Contain(_descriptors[0].Description));
            Assert.That(lines[4], Does.Contain("Path").And.Contain(_descriptors[0].Path));
            Assert.That(lines[5], Does.Contain("Version").And.Contain(_descriptors[0].Version));
            Assert.That(lines[6], Does.Contain("Batch mode").And.Contain("no"));
            Assert.That(lines[7], Does.Contain("Stream mode").And.Contain("yes"));
        }
    }
}