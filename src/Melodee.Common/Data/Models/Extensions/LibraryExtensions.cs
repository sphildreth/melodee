using Melodee.Common.Models;

namespace Melodee.Common.Data.Models.Extensions;

public static class LibraryExtensions
{
    public static void PurgePath(this Library library)
    {
        if (!Directory.Exists(library.Path))
        {
            return;
        }
        Directory.Delete(library.Path, true);
        if (!Directory.Exists(library.Path))
        {
            Directory.CreateDirectory(library.Path);
        }
    }
    
    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this Library library) =>
        new()
        {
            Path = library.Path,
            Name = library.Path
        };
}