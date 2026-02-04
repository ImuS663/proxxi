namespace Proxxi.Core.ProxyWriters;

public sealed class TsvProxyWriter(Stream stream, bool writeHeader = true) : DsvProxyWriter(stream, writeHeader, '\t');