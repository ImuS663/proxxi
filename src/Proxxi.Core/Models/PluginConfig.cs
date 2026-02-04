namespace Proxxi.Core.Models;

public sealed class PluginConfig : IEquatable<PluginConfig>
{
    public required string Id { get; init; }
    public string? Alias { get; init; }
    public required string Path { get; init; }
    public required string Version { get; init; }
    public bool Enabled { get; init; } = true;

    public IDictionary<string, string> Parameters { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool Equals(PluginConfig? other) =>
        other != null && StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id);

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is PluginConfig other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
}