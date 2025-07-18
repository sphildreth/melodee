using Melodee.Common.Models;
using Melodee.Common.Serialization;

namespace Melodee.Common.Services;

public class FileSystemService(ISerializer serializer) : IFileSystemService
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.Exists ? dirInfo.EnumerateFiles(searchPattern, searchOption).Select(f => f.FullName) : [];
    }

    public IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.Exists ? dirInfo.EnumerateDirectories(searchPattern, searchOption) : [];
    }

    public DateTime GetFileCreationTimeUtc(string filePath)
    {
        return File.GetCreationTimeUtc(filePath);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    public async Task<Album?> DeserializeAlbumAsync(string filePath, CancellationToken cancellationToken)
    {
        return await Album.DeserializeAndInitializeAlbumAsync(serializer, filePath, cancellationToken);
    }

    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path) ?? string.Empty;
    }

    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public string CombinePath(params string[] paths)
    {
        return Path.Combine(paths);
    }

    // Additional methods needed by ArtistService
    public async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public async Task WriteAllBytesAsync(string filePath, byte[] bytes, CancellationToken cancellationToken = default)
    {
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public void MoveDirectory(string sourcePath, string destinationPath)
    {
        Directory.Move(sourcePath, destinationPath);
    }

    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Directory.GetFiles(path, searchPattern, searchOption);
    }
}
