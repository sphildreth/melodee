namespace Melodee.Common.Models.Extensions;

public static class ReleaseFileExtensions
{
    public static string FileNameNoExtension(this ReleaseFile releaseFile)
    {
        return Path.GetFileNameWithoutExtension(releaseFile.FileSystemFileInfo.Name);
    }
}
