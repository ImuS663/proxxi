using Microsoft.Extensions.Options;
using Moq;
using Proxxi.Cli.Commands.Plugins;
using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugins;

[TestFixture(TestOf = typeof(PluginsCommand))]
public class PluginsCommandTests
{
    private static readonly PluginConfig[] Empty = [];

    private TestConsole _console;
    private IOptions<ProxxiPathsOptions> _options;
    private Mock<IPluginConfigProvider> _mock;

    private PluginConfig[] _plugins;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "plugins"));
        Directory.CreateDirectory(Path.Combine(dir, "plugins", "pack1"));
        File.WriteAllText(Path.Combine(dir, "plugins", "pack1", "test.plugin.dll"), "");

        _options = Options.Create(new ProxxiPathsOptions { ProxxiDir = Path.Combine(dir) });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (Directory.Exists(_options.Value.ProxxiDir))
            Directory.Delete(_options.Value.ProxxiDir, true);
    }

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();

        _plugins = TestData.Plugins.GetConfigs().ToArray();

        _mock = new Mock<IPluginConfigProvider>();
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

        var provider = _mock.Object;

        var command = new PluginsCommand(_console, provider, _options);

        var result = command.Execute(null!, new PluginsCommand.PluginsCommandSettings(), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("No installed plugins."));
        }
    }

    [Test]
    public void Execute_WhenThreePluginsInstalled_ReturnsZeroAndPrintsPlugins()
    {
        _mock.Setup(e => e.GetAll()).Returns(_plugins);

        var provider = _mock.Object;

        var command = new PluginsCommand(_console, provider, _options);

        var result = command.Execute(null!, new PluginsCommand.PluginsCommandSettings(), CancellationToken.None);

        var lines = _console.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);

            Assert.That(lines[1], Does.Contain("ID")
                .And.Contain("Alias")
                .And.Contain("Version")
                .And.Contain("Status"));

            Assert.That(lines[3], Does.Contain("test.plugin1")
                .And.Contain("<none>")
                .And.Contain("1.0.0")
                .And.Contain("disabled"));

            Assert.That(lines[4], Does.Contain("test.plugin2")
                .And.Contain("<none>")
                .And.Contain("1.2.0")
                .And.Contain("missing"));

            Assert.That(lines[5], Does.Contain("test.plugin3")
                .And.Contain("p-alias")
                .And.Contain("1.6.0")
                .And.Contain("enabled"));
        }
    }
}