using System.Diagnostics;
using Melodee.Common.Utility;

namespace Melodee.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static void Empty(this DirectoryInfo directory)
    {
        foreach(FileInfo file in directory.GetFiles()) file.Delete();
        foreach(DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
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
                    Trace.WriteLine($"Unable to delete [{directory}]: ex [{ ex}].");
                }
                catch (DirectoryNotFoundException) { }
            }
        }
        catch (UnauthorizedAccessException) { }
    }    
}
