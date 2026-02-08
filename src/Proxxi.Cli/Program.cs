// See https://aka.ms/new-console-template for more information

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Proxxi.Cli.Commands.Fetch;
using Proxxi.Cli.Infrastructure.Injection;
using Proxxi.Core.Extensions;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

string applicationName = Assembly.GetExecutingAssembly().GetName().Name ?? "proxxi";
string applicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unknown";

string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string defaultDir = Path.Combine(userDir, ".proxxi");

var proxxiDir = Environment.GetEnvironmentVariable("PROXXI_DIR") ?? defaultDir;

var services = new ServiceCollection();

services.AddProxxiPaths(proxxiDir);

services.AddSingleton(AnsiConsole.Console);

services.AddSingleton<IPluginLoader, PluginLoader>();

services.AddSingleton<IPluginConfigProvider, JsonPluginConfigProvider>();

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.SetApplicationName(applicationName);
    config.SetApplicationVersion(applicationVersion);

    config.AddCommand<FetchCommand>("fetch")
        .WithDescription("Fetch a proxies from source.")
        .WithExample("fetch", "test.plugin", "-o", "proxies.csv");

#if DEBUG
    config.ValidateExamples();
    config.PropagateExceptions();
#endif
});

return await app.RunAsync(args);