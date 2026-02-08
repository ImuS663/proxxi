using Microsoft.Extensions.DependencyInjection;

using OptionsFactory = Microsoft.Extensions.Options.Options;

using Proxxi.Core.Options;

namespace Proxxi.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProxxiPaths(this IServiceCollection services, string proxxiDir)
    {
        var options = OptionsFactory.Create(new ProxxiPathsOptions { ProxxiDir = proxxiDir });

        options.Value.EnsureCreated();

        services.AddSingleton(options);

        return services;
    }
}