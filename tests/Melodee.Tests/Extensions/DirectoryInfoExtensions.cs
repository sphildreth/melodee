using Melodee.Common.Models;

namespace Melodee.Tests.Extensions;

public static class DirectoryInfoExtensions
{
    public static FileSystemDirectoryInfo ToDirectorySystemInfo(this DirectoryInfo dirInfo)
    {
        var pp = Path.GetDirectoryName(dirInfo.FullName);
        var dir = new DirectoryInfo(pp ?? dirInfo.FullName);
        return new FileSystemDirectoryInfo
        {
            Path = dir.FullName,
            Name = dir.Name
        };
    }
}
