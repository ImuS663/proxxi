namespace Proxxi.Core.Options;

public sealed class ProxxiPathsOptions
{
    public required string ProxxiDir { get; init; }
    public string TmpDir => Path.Combine(ProxxiDir, "tmp");
    public string PluginsDir => Path.Combine(ProxxiDir, "plugins");
    public string PluginsFile => Path.Combine(ProxxiDir, "plugins.json");
}