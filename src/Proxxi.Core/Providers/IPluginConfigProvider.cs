using Proxxi.Core.Models;

namespace Proxxi.Core.Providers;

public interface IPluginConfigProvider
{
    public bool AliasExists(string alias, string? excludeId = null);
    public PluginConfig? Get(string id);
    public IReadOnlyCollection<PluginConfig> GetAll();
    public void Remove(string id);
    public void Save();
    public void Upsert(PluginConfig config);
}