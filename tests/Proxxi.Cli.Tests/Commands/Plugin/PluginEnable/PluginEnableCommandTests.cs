using Moq;
using Proxxi.Cli.Commands.Plugin.PluginDisable;
using Proxxi.Cli.Commands.Plugin.PluginEnable;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginEnable;

[TestFixture(TestOf = typeof(PluginEnableCommand))]
public class PluginEnableCommandTests
{
    private TestConsole _console;
    private Mock<IPluginConfigProvider> _mock;
    private PluginConfig[] _plugins;

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();

        _plugins = TestData.Plugins.GetConfigs().ToArray();

        _mock = new Mock<IPluginConfigProvider>();

        _mock.Setup(x => x.Get("test.plugin1")).Returns(_plugins[0]);
        _mock.Setup(x => x.Get("test.plugin2")).Returns(_plugins[1]);
        _mock.Setup(x => x.Get("test.plugin3")).Returns(_plugins[2]);
        _mock.Setup(x => x.Get("test.plugin4")).Returns((PluginConfig?)null);

        _mock.Setup(x => x.AliasExists("p-alias", "test.plugin1")).Returns(true);
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
        var command = new PluginEnableCommand(_console, _mock.Object);

        var settings = new PluginEnableCommand.PluginEnableCommandSettings { Id = "test.plugin4" };

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            command.Execute(null!, settings, CancellationToken.None);
        });

        Assert.That(exception.Message, Is.EqualTo("Plugin 'test.plugin4' not found."));
    }

    [Test]
    public void Execute_WhenPluginIsEnabled_ReturnsZeroAndNotSave()
    {
        var command = new PluginEnableCommand(_console, _mock.Object);

        var settings = new PluginEnableCommand.PluginEnableCommandSettings { Id = "test.plugin2" };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("Plugin already enabled."));
            _mock.Verify(x => x.Upsert(It.IsAny<PluginConfig>()), Times.Never);
            _mock.Verify(x => x.Save(), Times.Never);
        }
    }

    [Test]
    public void Execute_WhenPluginIsEnable_UpdatesPluginAndSave()
    {
        var command = new PluginEnableCommand(_console, _mock.Object);

        var settings = new PluginEnableCommand.PluginEnableCommandSettings { Id = "test.plugin1" };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("Plugin enabled."));

            _mock.Verify(x => x.Upsert(It.Is<PluginConfig>(c =>
                    c.Id == "test.plugin1" &&
                    c.Enabled == true)),
                Times.Once);

            _mock.Verify(x => x.Save(), Times.Once);
        }
    }
}