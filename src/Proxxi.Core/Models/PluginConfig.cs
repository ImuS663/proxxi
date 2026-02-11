namespace Proxxi.Core.Models;

public sealed class PluginConfig : IEquatable<PluginConfig>
{
    public required string Id { get; set; }
    public string? Alias { get; set; }
    public required string Path { get; set; }
    public required string Version { get; set; }
    public bool Enabled { get; set; } = true;

    public IDictionary<string, string> Parameters { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool Equals(PluginConfig? other) =>
        other != null && StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id);

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is PluginConfig other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
}