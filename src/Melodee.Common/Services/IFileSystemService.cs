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

    // Additional methods needed by ArtistService
    Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default);
    Task WriteAllBytesAsync(string filePath, byte[] bytes, CancellationToken cancellationToken = default);
    void CreateDirectory(string path);
    bool FileExists(string path);
    void DeleteFile(string path);
    void MoveDirectory(string sourcePath, string destinationPath);
    string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);
}
