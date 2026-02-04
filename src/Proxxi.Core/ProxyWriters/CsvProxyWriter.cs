namespace Proxxi.Core.ProxyWriters;

public sealed class CsvProxyWriter(Stream stream, bool writeHeader = true) : DsvProxyWriter(stream, writeHeader, ',');