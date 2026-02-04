namespace Proxxi.Plugin.Loader.Models;

public record PluginParameter(string Name, string Description, bool Required = false)
{
    public virtual bool Equals(PluginParameter? other) =>
        other != null && Name == other.Name;

    public override int GetHashCode() =>
        Name.GetHashCode();
}