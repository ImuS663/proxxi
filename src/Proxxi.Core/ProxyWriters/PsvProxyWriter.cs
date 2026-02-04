namespace Proxxi.Core.ProxyWriters;

public sealed class PsvProxyWriter(Stream stream, bool writeHeader = true) : DsvProxyWriter(stream, writeHeader, '|');