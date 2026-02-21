// See https://aka.ms/new-console-template for more information

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Proxxi.Cli.Commands.Fetch;
using Proxxi.Cli.Commands.Install;
using Proxxi.Cli.Commands.Plugin;
using Proxxi.Cli.Commands.Plugin.PluginAlias;
using Proxxi.Cli.Commands.Plugin.PluginDisable;
using Proxxi.Cli.Commands.Plugin.PluginEnable;
using Proxxi.Cli.Commands.Plugin.PluginInfo;
using Proxxi.Cli.Commands.Plugin.PluginParameter;
using Proxxi.Cli.Commands.Plugin.PluginParameters;
using Proxxi.Cli.Commands.Plugins;
using Proxxi.Cli.Commands.Remove;
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

    config.AddBranch<PluginCommandSettings>("plugin", pluginConfig =>
    {
        pluginConfig.SetDescription("Manage plugins.");

        pluginConfig.SetDefaultCommand<PluginInfoCommand>();

        pluginConfig.AddCommand<PluginAliasCommand>("alias")
            .WithDescription("Set or remove an alias for a plugin.");

        pluginConfig.AddCommand<PluginEnableCommand>("enable")
            .WithDescription("Enable a plugin.");

        pluginConfig.AddCommand<PluginDisableCommand>("disable")
            .WithDescription("Disable a plugin.");

        pluginConfig.AddCommand<PluginParameterCommand>("parameter")
            .WithDescription("Set or remove a parameter for a plugin.");

        pluginConfig.AddCommand<PluginParametersCommand>("parameters")
            .WithDescription("List parameters for a plugin.");

        pluginConfig.AddCommand<PluginInfoCommand>("info")
            .WithDescription("Show information about a plugin.");
    });

    config.AddCommand<PluginsCommand>("plugins")
        .WithDescription("List installed plugins.");

    config.AddCommand<InstallCommand>("install")
        .WithDescription("Install a plugins with (.pxp) package.");

    config.AddCommand<RemoveCommand>("remove")
        .WithDescription("Remove a plugins package.");

#if DEBUG
    config.ValidateExamples();
    config.PropagateExceptions();
#endif
});

return await app.RunAsync(args);