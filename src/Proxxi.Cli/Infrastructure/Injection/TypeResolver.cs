using Spectre.Console.Cli;

namespace Proxxi.Cli.Infrastructure.Injection;

public class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object? Resolve(Type? type) =>
        type != null ? provider.GetService(type) : null;
}