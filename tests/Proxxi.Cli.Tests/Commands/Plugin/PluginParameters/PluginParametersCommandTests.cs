using Microsoft.Extensions.DependencyInjection;

using Moq;

using Proxxi.Cli.Commands.Plugin.PluginParameters;
using Proxxi.Cli.Infrastructure.Injection;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Extensions;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginParameters;

[TestFixture(TestOf = typeof(PluginParametersCommand))]
public class PluginParametersCommandTests
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

        app.SetDefaultCommand<PluginParametersCommand>();

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

        app.SetDefaultCommand<PluginParametersCommand>();

        var result = app.Run("test.plugin2", "--desc");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.EqualTo(-1));
            Assert.That(result.Output, Does.Contain("Plugin 'test.plugin2' is not loaded."));
        }
    }

    [Test]
    public void Execute_WhenPluginIsInstalled_PrintsPluginParameters()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginParametersCommand>();

        var result = app.Run("test.plugin3");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);
            Assert.That(result.Output, Does.Contain("key").And.Contain("value"));
        }
    }

    [Test]
    public void Execute_WhenPluginIsInstalledAndDescFlag_PrintsPluginParametersDescriptions()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginParametersCommand>();

        var result = app.Run("test.plugin3", "--desc");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);

            Assert.That(result.Output, Does.Contain("key")
                .And.Contain("Key for test plugin")
                .And.Contain("required"));

            Assert.That(result.Output, Does.Contain("page").And.Contain("Page for test plugin"));
        }
    }
}