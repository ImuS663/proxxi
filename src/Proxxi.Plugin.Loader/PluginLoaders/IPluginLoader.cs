using Proxxi.Plugin.Loader.Models;

namespace Proxxi.Plugin.Loader.PluginLoaders;

public interface IPluginLoader
{
    public IReadOnlyCollection<PluginDescriptor> LoadPlugins(IEnumerable<string> paths);
}