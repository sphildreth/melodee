namespace Melodee.Common.Models.Extensions;

public static class AlbumFileExtensions
{
    public static string FileNameNoExtension(this AlbumFile albumFile)
    {
        return Path.GetFileNameWithoutExtension(albumFile.FileSystemFileInfo.Name);
    }
}
