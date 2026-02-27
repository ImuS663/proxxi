using Proxxi.Plugin.Loader.Models;

namespace Proxxi.Plugin.Loader.PluginLoaders;

public interface IPluginLoader
{
    public IReadOnlyCollection<PluginDescriptor> LoadPlugins(IEnumerable<string> paths);
    public IReadOnlyCollection<PluginDescriptor> LoadPlugins(string path);
    public PluginDescriptor? LoadPlugin(string path, string id);
}