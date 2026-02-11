using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;

namespace Proxxi.Core.Providers;

public class JsonPluginConfigProvider : IPluginConfigProvider
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _pluginsFile;
    private readonly List<PluginConfig> _configs;

    public JsonPluginConfigProvider(IOptions<ProxxiPathsOptions> options)
    {
        _pluginsFile = options.Value.PluginsFile;

        var json = File.ReadAllText(_pluginsFile);

        _configs = JsonSerializer.Deserialize<List<PluginConfig>>(json, _options) ?? [];
    }

    public bool AliasExists(string alias, string? excludeId = null) =>
        _configs.Any(c =>
            c.Alias != null &&
            Comparer.Equals(c.Alias, alias) &&
            (excludeId != null || !Comparer.Equals(c.Id, excludeId)));

    public PluginConfig? Get(string id) =>
        _configs.FirstOrDefault(c => Comparer.Equals(c.Id, id) || (c.Alias != null && Comparer.Equals(c.Alias, id)));

    public IReadOnlyCollection<PluginConfig> GetAll() =>
        _configs;

    public void Remove(string id) =>
        _configs.RemoveAll(c => Comparer.Equals(c.Id, id));

    public void Save()
    {
        var json = JsonSerializer.Serialize(_configs, _options);

        var tmp = _pluginsFile + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, _pluginsFile, true);
    }

    public void Upsert(PluginConfig config)
    {
        var index = _configs.FindIndex(c => Comparer.Equals(c.Id, config.Id));

        if (index >= 0)
            _configs[index] = config;
        else
            _configs.Add(config);
    }
}