using System.Text.RegularExpressions;
using Melodee.Common.Extensions;
using SerilogTimings;

namespace Melodee.Common.Utility;

public static partial class DirectoryHelper
{
    public static void DeleteFolderIfEmpty(DirectoryInfo? dir)
    {
        if (dir is null || dir.EnumerateFiles().Any() || dir.EnumerateDirectories().Any())
        {
            return;
        }
        DirectoryInfo? parent = dir.Parent;
        dir.Delete();

        // Climb up to the parent
        DeleteFolderIfEmpty(parent);
    }    
    
    public static void DeleteEmptyFolders(string dir)
    {
        foreach (var subDir in System.IO.Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories))
        {
            DeleteFolderIfEmpty(new DirectoryInfo(subDir));
        }
    }    
    
    public static void DeleteDirectory(string targetDir)
    {
        try
        {
            string[] files = System.IO.Directory.GetFiles(targetDir);
            string[] dirs = System.IO.Directory.GetDirectories(targetDir);
            foreach (string file in files)
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
                System.IO.File.Delete(file);
            }
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
            System.IO.Directory.Delete(targetDir, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Deleting [{targetDir}] [{ex.Message}]");
        }
    }    
    
    public static void MoveFolder(string sourceDirectory, string destinationDirectory)
    {
        if (!System.IO.Directory.Exists(destinationDirectory))
        {
            System.IO.Directory.CreateDirectory(destinationDirectory);
        }
        string[] files = System.IO.Directory.GetFiles(sourceDirectory);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(destinationDirectory, name);
            if (string.Equals(name, "ted.data.json", StringComparison.OrdinalIgnoreCase))
            {                    
                using (Operation.Time("Deleting File [{File}]", file))
                {
                    System.IO.File.Delete(file);
                }
            }
            else
            {
                using (Operation.Time("Moving File [{File}] To [{Dest}]", file, dest))
                {
                    System.IO.File.Move(file, dest, true);
                }
            }
        }
        string[] folders = System.IO.Directory.GetDirectories(sourceDirectory);
        foreach (string folder in folders)
        {
            string name = Path.GetFileName(folder);
            string dest = Path.Combine(destinationDirectory, name);
            MoveFolder(folder, dest);
        }
        if (IsDirectoryEmpty(sourceDirectory))
        {
            System.IO.Directory.Delete(sourceDirectory);
        }
    }    
    
    public static bool IsDirectoryEmpty(string path) => !System.IO.Directory.EnumerateFileSystemEntries(path).Any();  
    
    public static bool IsDirectoryMediaDirectory(string releaseDirectory, string? dir)
    {
        if (dir?.Nullify() == null)
        {
            return false;
        }
        return IsDirectoryMediaDirectoryRegex().IsMatch(dir);
    }

    [GeneratedRegex(@"(\s*(CD[.\S]*[0-9])|(CD\s[0-9])+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex IsDirectoryMediaDirectoryRegex();
}