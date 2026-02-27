using System.ComponentModel;
using System.Reflection;

using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Sdk.Attributes;
using Proxxi.Plugin.Sdk.ProxySources;

namespace Proxxi.Plugin.Loader.PluginLoaders;

public sealed class PluginLoader : IPluginLoader
{
    public IReadOnlyCollection<PluginDescriptor> LoadPlugins(IEnumerable<string> paths)
    {
        var descriptors = new List<PluginDescriptor>();

        foreach (var path in paths)
            descriptors.AddRange(LoadPlugins(path));

        return descriptors;
    }

    public PluginDescriptor? LoadPlugin(string path, string id) =>
        LoadPlugins(path).FirstOrDefault(pd => StringComparer.OrdinalIgnoreCase.Equals(pd.Id, id));

    public IReadOnlyCollection<PluginDescriptor> LoadPlugins(string path)
    {
        var descriptors = new List<PluginDescriptor>();

        var discoveryContext = new PluginLoadContext(path);

        var loadedAssembly = discoveryContext.LoadFromAssemblyPath(path);

        var pluginTypes = ExtractPluginTypes(loadedAssembly);

        discoveryContext.Unload();

        foreach ((Type type, ProxySourceAttribute attribute) in pluginTypes)
        {
            var context = new PluginLoadContext(path);
            var pluginAssembly = context.LoadFromAssemblyPath(path);

            var pluginType = pluginAssembly.GetType(type.FullName!, true)!;

            var description = pluginType.GetCustomAttribute<DescriptionAttribute>();

            var parameters = pluginType.GetCustomAttributes<ParameterProxySourceAttribute>()
                .Select(a => new PluginParameter(a.Name, a.Description, a.Required))
                .ToHashSet();

            var version = pluginType.Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

            descriptors.Add(new PluginDescriptor(attribute.Id, attribute.Name, description?.Description ?? "",
                version, path, attribute.HideBatch, attribute.HideStream, parameters, pluginType, context));
        }

        return descriptors;
    }

    private static IEnumerable<(Type Type, ProxySourceAttribute Attribute)> ExtractPluginTypes(Assembly loadedAssembly)
    {
        return loadedAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsAssignableTo(typeof(IProxySource)))
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<ProxySourceAttribute>()))
            .OfType<(Type Type, ProxySourceAttribute Attribute)>();
    }
}