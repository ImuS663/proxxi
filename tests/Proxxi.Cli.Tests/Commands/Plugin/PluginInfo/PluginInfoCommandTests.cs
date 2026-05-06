using Microsoft.Extensions.DependencyInjection;

using Moq;

using Proxxi.Cli.Commands.Plugin.PluginInfo;
using Proxxi.Cli.Infrastructure.Injection;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Extensions;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginInfo;

[TestFixture(TestOf = typeof(PluginInfoCommand))]
public class PluginInfoCommandTests
{
    private readonly ServiceCollection _services = [];

    private TestConsole _console;
    private Mock<IPluginConfigProvider> _mockPluginConfigProvider;
    private Mock<IPluginLoader> _mockPluginLoader;
    private PluginConfig[] _configs;
    private PluginDescriptor[] _descriptors;

    private string _proxxiDir;

    private string PluginsDir => Path.Combine(_proxxiDir, "plugins");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _proxxiDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
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

        _services.AddProxxiPaths(_proxxiDir);
        _services.AddSingleton(_console);
        _services.AddSingleton(_mockPluginConfigProvider.Object);
        _services.AddSingleton(_mockPluginLoader.Object);
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
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginInfoCommand>();

        var result = app.Run("test.plugin4");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.EqualTo(-1));
            Assert.That(result.Output, Does.Contain("Plugin 'test.plugin4' is not installed."));
        }
    }

    [Test]
    public void Execute_WhenPluginIsNotLoaded_ThrowsInvalidOperationException()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginInfoCommand>();

        var result = app.Run("test.plugin2");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.EqualTo(-1));
            Assert.That(result.Output, Does.Contain("Plugin 'test.plugin2' is not loaded."));
        }
    }

    [Test]
    public void Execute_WhenPluginIsInstalled_PrintsPluginInfo()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginInfoCommand>();

        var result = app.Run("test.plugin1");

        var lines = result.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);
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