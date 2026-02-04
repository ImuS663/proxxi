using System.Text.Json;
using System.Text.Json.Serialization;

using Proxxi.Core.Extensions;
using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.ProxyWriters.Converters;

internal sealed class ProtocolsJsonConverter : JsonConverter<Protocols>
{
    public override Protocols Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("ProtocolsJsonConverter is write-only and cannot be used for deserialization.");
    }

    public override void Write(Utf8JsonWriter writer, Protocols value, JsonSerializerOptions options)
    {
        var protocols = value.ToStrings();

        writer.WriteStartArray();

        foreach (var protocol in protocols)
            writer.WriteStringValue(protocol);

        writer.WriteEndArray();
    }
}