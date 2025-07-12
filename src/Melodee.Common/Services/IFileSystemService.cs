using Melodee.Common.Models;

namespace Melodee.Common.Services;

public interface IFileSystemService
{
    bool DirectoryExists(string path);
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);
    DateTime GetFileCreationTimeUtc(string filePath);
    void DeleteDirectory(string path, bool recursive);
    Task<Album?> DeserializeAlbumAsync(string filePath, CancellationToken cancellationToken);
    string GetDirectoryName(string path);
    string GetFileName(string path);
    string CombinePath(params string[] paths);
}
