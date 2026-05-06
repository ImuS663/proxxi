using Microsoft.Extensions.DependencyInjection;

using Moq;

using Proxxi.Cli.Commands.Plugin.PluginDisable;
using Proxxi.Cli.Infrastructure.Injection;
using Proxxi.Cli.Tests.TestData;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;

using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginDisable;

[TestFixture(TestOf = typeof(PluginDisableCommand))]
public class PluginDisableCommandTests
{
    private readonly ServiceCollection _services = [];

    private TestConsole _console;
    private Mock<IPluginConfigProvider> _mock;
    private PluginConfig[] _plugins;

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();

        _plugins = PluginTestData.GetConfigs().ToArray();

        _mock = new Mock<IPluginConfigProvider>();

        _mock.Setup(x => x.Get("test.plugin1")).Returns(_plugins[0]);
        _mock.Setup(x => x.Get("test.plugin2")).Returns(_plugins[1]);
        _mock.Setup(x => x.Get("test.plugin3")).Returns(_plugins[2]);
        _mock.Setup(x => x.Get("test.plugin4")).Returns((PluginConfig?)null);

        _mock.Setup(x => x.AliasExists("p-alias", "test.plugin1")).Returns(true);

        _services.AddSingleton(_console);
        _services.AddSingleton(_mock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _console.Dispose();

        _mock.Reset();
    }

    [Test]
    public void Execute_WhenPluginIsNotFound_ThrowsInvalidOperationException()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginDisableCommand>();

        var result = app.Run("test.plugin4");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.EqualTo(-1));
            Assert.That(result.Output, Does.Contain("Plugin 'test.plugin4' not found."));
        }
    }

    [Test]
    public void Execute_WhenPluginIsDisabled_ReturnsZeroAndNotSave()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginDisableCommand>();

        var result = app.Run("test.plugin1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);
            Assert.That(result.Output, Does.Contain("Plugin already disabled."));
            _mock.Verify(x => x.UpsertAndSave(It.IsAny<PluginConfig>()), Times.Never);
        }
    }

    [Test]
    public void Execute_WhenPluginIsDisable_UpdatesPluginAndSave()
    {
        var app = new CommandAppTester(new TypeRegistrar(_services));

        app.SetDefaultCommand<PluginDisableCommand>();

        var result = app.Run("test.plugin2");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ExitCode, Is.Zero);
            Assert.That(result.Output, Does.Contain("Plugin disabled."));

            _mock.Verify(x => x.UpsertAndSave(It.Is<PluginConfig>(c =>
                    c.Id == "test.plugin2" &&
                    c.Enabled == false)),
                Times.Once);
        }
    }
}