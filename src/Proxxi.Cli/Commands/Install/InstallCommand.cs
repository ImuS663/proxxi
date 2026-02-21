using System.Formats.Tar;
using System.IO.Compression;

using Microsoft.Extensions.Options;

using Proxxi.Core.Models;
using Proxxi.Core.Options;
using Proxxi.Core.Providers;
using Proxxi.Plugin.Loader.Models;
using Proxxi.Plugin.Loader.PluginLoaders;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Proxxi.Cli.Commands.Install;

public sealed class InstallCommand(
    IAnsiConsole console,
    IPluginConfigProvider configProvider,
    IPluginLoader pluginLoader,
    IOptions<ProxxiPathsOptions> options
) : Command<InstallCommandSettings>
{
    private readonly ProxxiPathsOptions _pathOptions = options.Value;

    public override int Execute(CommandContext context, InstallCommandSettings settings, CancellationToken ct)
    {
        var fileName = Path.GetFileNameWithoutExtension(settings.Path);
        var tempDir = Path.Combine(_pathOptions.TmpDir, Guid.NewGuid().ToString("N"));
        var endDir = Path.Combine(_pathOptions.PluginsDir, fileName);

        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);

        Directory.CreateDirectory(tempDir);

        try
        {
            using var stream = File.OpenRead(settings.Path);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress, true);
            TarFile.ExtractToDirectory(gzip, tempDir, false);

            var pluginDlls = Directory.GetFiles(tempDir, "*.plugin.dll", SearchOption.AllDirectories);

            switch (pluginDlls.Length)
            {
                case 0:
                    throw new InvalidOperationException($"Plugin '{fileName}' does not contain a plugin assembly.");
                case > 1:
                    throw new InvalidOperationException($"Plugin '{fileName}' contains multiple plugin assemblies.");
            }

            var pluginDll = pluginDlls[0];

            var descriptors = pluginLoader.LoadPlugins([pluginDll]);

            if (descriptors.Count == 0)
                throw new InvalidOperationException($"Plugin '{fileName}' does not contain valid plugins.");

            var ids = descriptors.Select(pd => pd.Id).Distinct();

            var exist = configProvider.GetAll().Any(c => ids.Contains(c.Id));

            if (exist && !settings.Update)
            {
                console.MarkupLine(
                    $"[yellow]Warning:[/] Package '{fileName}' is already installed. Use --update to reinstall/update.");

                return 0;
            }

            if (Directory.Exists(descriptors.First().Path))
                Directory.Delete(descriptors.First().Path, true);

            if (Directory.Exists(endDir))
                Directory.Delete(endDir, true);

            Directory.Move(tempDir, endDir);

            UpdatePluginConfigurations(descriptors, fileName, tempDir, pluginDll);

            console.MarkupLine($"[green]✓[/] Plugins package '{fileName}' installed.");

            return 0;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    public override ValidationResult Validate(CommandContext context, InstallCommandSettings settings)
    {
        return !string.Equals(Path.GetExtension(settings.Path), ".pxp", StringComparison.OrdinalIgnoreCase)
            ? ValidationResult.Error("File must be a .pxp file.")
            : base.Validate(context, settings);
    }

    private void UpdatePluginConfigurations(IReadOnlyCollection<PluginDescriptor> descriptors, string fileName,
        string tempDir, string pluginDll)
    {
        foreach (var descriptor in descriptors)
        {
            var config = new PluginConfig
            {
                Id = descriptor.Id,
                Path = Path.Combine(fileName, Path.GetRelativePath(tempDir, pluginDll)),
                Version = descriptor.Version,
            };

            configProvider.Upsert(config);
        }

        configProvider.Save();
    }
}