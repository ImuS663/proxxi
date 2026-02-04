using Proxxi.Core.Models;

namespace Proxxi.Core.Providers;

public interface IPluginConfigProvider
{
    public PluginConfig? Get(string id);
    public void Remove(string id);
    public void Save();
    public void Upsert(PluginConfig config);
}