namespace Proxxi.Plugin.Loader.Models;

public record PluginDescriptor(
    string Id,
    string Name,
    string Description,
    string Version,
    string Path,
    bool HideBatch,
    bool HideStream,
    IReadOnlyCollection<PluginParameter> Parameters,
    Type ProxySourceType
)
{
    private readonly PluginLoadContext? _loadContext;

    internal PluginDescriptor(string id, string name, string description, string version, string path,
        bool hideBatch, bool hideStream, IReadOnlyCollection<PluginParameter> parameters, Type proxySourceType,
        PluginLoadContext loadContext) : this(id, name, description, version, path, hideBatch,
        hideStream, parameters, proxySourceType)
    {
        _loadContext = loadContext;
    }
}