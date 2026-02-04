using Proxxi.Plugin.Sdk.Models;

namespace Proxxi.Core.Extensions;

public static class ProtocolsExtension
{
    public static IEnumerable<string> ToStrings(this Protocols protocols) =>
        Enum.GetValues<Protocols>()
            .Where(protocol => protocol != Protocols.None)
            .Where(protocol => (protocols & protocol) == protocol)
            .Select(protocol => protocol.ToString().ToLowerInvariant());
}