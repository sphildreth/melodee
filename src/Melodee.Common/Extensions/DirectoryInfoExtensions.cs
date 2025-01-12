using System.Diagnostics;
using Melodee.Common.Utility;

namespace Melodee.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static void Empty(this DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles())
        {
            file.Delete();
        }

        foreach (var subDirectory in directory.GetDirectories())
        {
            subDirectory.Delete(true);
        }
    }

    public static bool FileIsLikelyDuplicateByCrcAndExtension(this DirectoryInfo directory, FileInfo file)
    {
        if (!directory.Exists || !file.Exists)
        {
            return false;
        }

        var crc = Crc32.Calculate(file);
        if (crc.Nullify() == null)
        {
            return false;
        }

        return directory.EnumerateFiles($"*{file.Extension}", SearchOption.AllDirectories).Any(file => Crc32.Calculate(file) == crc);
    }

    public static bool DoesDirectoryHaveImageFiles(this DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return false;
        }

        return directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Any(x => FileHelper.IsFileImageType(x.Extension));
    }

    public static bool DoesDirectoryHaveMediaFiles(this DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return false;
        }

        return directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Any(x => FileHelper.IsFileMediaType(x.Extension));
    }

    public static void DeleteIfEmpty(this DirectoryInfo directory)
    {
        try
        {
            foreach (var d in directory.EnumerateDirectories())
            {
                d.DeleteIfEmpty();
            }

            var entries = directory.EnumerateFileSystemInfos();

            if (!entries.Any())
            {
                try
                {
                    directory.Delete();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Trace.WriteLine($"Unable to delete [{directory}]: ex [{ex}].");
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
