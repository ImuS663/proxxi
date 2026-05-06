using Microsoft.Extensions.DependencyInjection;

using Moq;

using Proxxi.Cli.Commands.Plugins;
using Proxxi.Cli.Infrastructure.Injection;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Extensions;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;

using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugins;

[TestFixture(TestOf = typeof(PluginsCommand))]
public class PluginsCommandTests
{
    private static readonly PluginConfig[] Empty = [];

    private readonly ServiceCollection _services = [];

    private TestConsole _console;
    private Mock<IPluginConfigProvider> _mock;

    private PluginConfig[] _plugins;

    private string _proxxiDir;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _proxxiDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_proxxiDir);
        Directory.CreateDirectory(Path.Combine(_proxxiDir, "plugins"));
        Directory.CreateDirectory(Path.Combine(_proxxiDir, "plugins", "pack1"));
        File.WriteAllText(Path.Combine(_proxxiDir, "plugins", "pack1", "test.plugin.dll"), "");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_proxxiDir))
            Directory.Delete(_proxxiDir, true);
    }

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();

        _plugins = PluginTestData.GetConfigs().ToArray();

        _mock = new Mock<IPluginConfigProvider>();

        _services.AddProxxiPaths(_proxxiDir);
        _services.AddSingleton(_console);
    }

    [TearDown]
    public void TearDown()
    {
        _console.Dispose();

        _mock.Reset();
    }

    [Test]
    public void Execute_WhenNoPluginsInstalled_ReturnsZeroAndPrintsMessage()
    {
        _mock.Setup(e => e.GetAll()).Returns(Empty);

        _services.AddSingleton(_mock.Object);

        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginsCommand>();

        var result = app.Run();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);
            Assert.That(result.Output, Does.Contain("No installed plugins."));
        }
    }

    [Test]
    public void Execute_WhenThreePluginsInstalled_ReturnsZeroAndPrintsPlugins()
    {
        _mock.Setup(e => e.GetAll()).Returns(_plugins);

        _services.AddSingleton(_mock.Object);

        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginsCommand>();

        var result = app.Run();

        var lines = result.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);

            Assert.That(lines[0], Does.Contain("ID")
                .And.Contain("Alias")
                .And.Contain("Version")
                .And.Contain("Status"));

            Assert.That(lines[2], Does.Contain("test.plugin1")
                .And.Contain("<none>")
                .And.Contain("1.0.0")
                .And.Contain("disabled"));

            Assert.That(lines[3], Does.Contain("test.plugin2")
                .And.Contain("<none>")
                .And.Contain("1.2.0")
                .And.Contain("missing"));

            Assert.That(lines[4], Does.Contain("test.plugin3")
                .And.Contain("p-alias")
                .And.Contain("1.6.0")
                .And.Contain("enabled"));
        }
    }
}