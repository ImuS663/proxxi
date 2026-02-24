using Moq;
using Proxxi.Cli.Commands.Plugin.PluginAlias;
using Proxxi.Core.Models;
using Proxxi.Core.Providers;
using Spectre.Console.Testing;

namespace Proxxi.Cli.Tests.Commands.Plugin.PluginAlias;

[TestFixture(TestOf = typeof(PluginAliasCommand))]
public class PluginAliasCommandTests
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
    public void Execute_WhenPluginIsNotInstalled_ThrowsInvalidOperationException()
    {
        var command = new PluginAliasCommand(_console, _mock.Object);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            command.Execute(null!, new PluginAliasCommandSettings { Id = "test.plugin4" }, CancellationToken.None);
        });

        Assert.That(exception.Message, Is.EqualTo("Plugin 'test.plugin4' is not installed."));
    }

    [Test]
    public void Execute_WhenAliasAlreadyExists_ThrowsInvalidOperationException()
    {
        var command = new PluginAliasCommand(_console, _mock.Object);

        var settings = new PluginAliasCommandSettings { Id = "test.plugin1", Value = "p-alias" };

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            command.Execute(null!, settings, CancellationToken.None);
        });

        Assert.That(exception.Message, Is.EqualTo("Alias 'p-alias' is already in use."));
    }

    [Test]
    public void Execute_WhenAliasIsSet_UpdatesPluginAndSave()
    {
        var command = new PluginAliasCommand(_console, _mock.Object);

        var settings = new PluginAliasCommandSettings { Id = "test.plugin1", Value = "new-alias" };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("Alias updated: <none> → new-alias"));

            _mock.Verify(x => x.Upsert(It.Is<PluginConfig>(c =>
                    c.Id == "test.plugin1" &&
                    c.Alias == "new-alias" &&
                    c.Version == "1.0.0")),
                Times.Once);

            _mock.Verify(x => x.Save(), Times.Once);
        }
    }

    [Test]
    public void Execute_WhenRemovingAliasButPluginHasNone_ReturnsZeroAndNotSave()
    {
        var command = new PluginAliasCommand(_console, _mock.Object);

        var settings = new PluginAliasCommandSettings { Id = "test.plugin1", Remove = true };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("Plugin has no alias to remove."));
            _mock.Verify(x => x.Upsert(It.IsAny<PluginConfig>()), Times.Never);
            _mock.Verify(x => x.Save(), Times.Never);
        }
    }

    [Test]
    public void Execute_WhenRemovingAlias_ClearsAliasAndSave()
    {
        var command = new PluginAliasCommand(_console, _mock.Object);

        var settings = new PluginAliasCommandSettings { Id = "test.plugin3", Remove = true };

        var result = command.Execute(null!, settings, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Zero);
            Assert.That(_console.Output, Does.Contain("Alias removed."));

            _mock.Verify(x => x.Upsert(It.Is<PluginConfig>(c =>
                    c.Id == "test.plugin3" &&
                    c.Alias == null)),
                Times.Once);

            _mock.Verify(x => x.Save(), Times.Once);
        }
    }
}