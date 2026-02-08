using Proxxi.Core.Options;

namespace Proxxi.Core.Extensions;

public static class ProxxiPathsOptionsExtension
{
    public static void EnsureCreated(this ProxxiPathsOptions options)
    {
        CreateDirectory(options.ProxxiDir, true);
        CreateDirectory(options.TmpDir);
        CreateDirectory(options.PluginsDir);

        EnsureFile(options.PluginsFile, "[]");
    }

    private static void CreateDirectory(string path, bool hiddenOnWindows = false)
    {
        if (Directory.Exists(path))
            return;

        var info = Directory.CreateDirectory(path);

        if (hiddenOnWindows && OperatingSystem.IsWindows())
            info.Attributes |= FileAttributes.Hidden;
    }

    private static void EnsureFile(string path, string defaultContent = "")
    {
        if (!File.Exists(path))
            File.WriteAllText(path, defaultContent);
    }
}