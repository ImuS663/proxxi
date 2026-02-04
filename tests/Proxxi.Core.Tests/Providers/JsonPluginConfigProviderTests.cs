using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;

namespace Proxxi.Core.Tests.Providers;

[TestFixture(TestOf = typeof(JsonPluginConfigProvider))]
public class JsonPluginConfigProviderTests
{
    private string _tempDir;

    private IOptions<ProxxiPathsOptions> _options;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var proxxiPathOptions = new ProxxiPathsOptions { ProxxiDir = _tempDir };

        File.WriteAllText(proxxiPathOptions.PluginsFile, "[]");

        _options = Microsoft.Extensions.Options.Options.Create(proxxiPathOptions);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Test]
    public void Get_WhenPluginDoesNotExist_ReturnsNull()
    {
        var provider = new JsonPluginConfigProvider(_options);

        var result = provider.Get("nonexisting.plugin");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Get_WhenPluginExist_ReturnsNull()
    {
        const string json = """
                            [
                              {
                                "id": "test.plugin",
                                "path": "plugins/test",
                                "version": "1.0.0",
                                "enabled": true
                              }
                            ]
                            """;

        File.WriteAllText(_options.Value.PluginsFile, json);

        var provider = new JsonPluginConfigProvider(_options);

        var result = provider.Get("test.plugin");

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo("test.plugin"));
            Assert.That(result.Path, Is.EqualTo("plugins/test"));
            Assert.That(result.Version, Is.EqualTo("1.0.0"));
            Assert.That(result.Enabled, Is.True);
        }
    }

    [Test]
    public void Upsert_WhenNewPlugin_AddsPlugin()
    {
        var provider = new JsonPluginConfigProvider(_options);

        provider.Upsert(new PluginConfig
        {
            Id = "test.plugin", Path = "plugins/test", Version = "1.0.0", Enabled = true
        });

        var result = provider.Get("test.plugin");

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo("test.plugin"));
            Assert.That(result.Path, Is.EqualTo("plugins/test"));
            Assert.That(result.Version, Is.EqualTo("1.0.0"));
            Assert.That(result.Enabled, Is.True);
        }
    }

    [Test]
    public void Upsert_WhenPluginExists_UpdatesPlugin()
    {
        const string json = """
                            [
                              {
                                "id": "test.plugin",
                                "path": "old/plugins/test",
                                "version": "1.0.0",
                                "enabled": true
                              }
                            ]
                            """;

        File.WriteAllText(_options.Value.PluginsFile, json);

        var provider = new JsonPluginConfigProvider(_options);

        provider.Upsert(new PluginConfig
        {
            Id = "test.plugin",
            Alias = "plugin-alias",
            Path = "new/plugins/test",
            Version = "2.0.0",
            Enabled = false
        });

        var result = provider.Get("test.plugin");

        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo("test.plugin"));
            Assert.That(result.Alias, Is.EqualTo("plugin-alias"));
            Assert.That(result.Path, Is.EqualTo("new/plugins/test"));
            Assert.That(result.Version, Is.EqualTo("2.0.0"));
            Assert.That(result.Enabled, Is.False);
        }
    }

    [Test]
    public void Remove_RemovesPlugin()
    {
        const string json = """
                            [
                              {
                                "id": "test.plugin",
                                "path": "old/plugins/test",
                                "version": "1.0.0",
                                "enabled": true
                              }
                            ]
                            """;

        File.WriteAllText(_options.Value.PluginsFile, json);

        var provider = new JsonPluginConfigProvider(_options);

        provider.Remove("test.plugin");

        var result = provider.Get("test.plugin");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Save_WritesUpdatedConfigToFile()
    {
        var provider = new JsonPluginConfigProvider(_options);

        provider.Upsert(new PluginConfig
        {
            Id = "test.plugin", Path = "plugins/test", Version = "1.0.0", Enabled = true
        });

        provider.Save();

        var json = File.ReadAllText(_options.Value.PluginsFile);

        Assert.That(json, Does.Contain("test.plugin"));
        Assert.That(json, Does.Contain("1.0.0"));
    }
}