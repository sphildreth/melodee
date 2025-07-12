using Melodee.Common.Models;
using Melodee.Common.Services;

namespace Melodee.Tests.Services;

/// <summary>
/// Mock implementation of IFileSystemService for testing purposes.
/// Allows simulation of various file system scenarios without actual file I/O.
/// </summary>
public class MockFileSystemService : IFileSystemService
{
    private readonly Dictionary<string, bool> _directoryExists = new();
    private readonly Dictionary<string, List<string>> _directoryFiles = new();
    private readonly Dictionary<string, List<DirectoryInfo>> _directorySubdirectories = new();
    private readonly Dictionary<string, DateTime> _fileCreationTimes = new();
    private readonly Dictionary<string, Album> _albumCache = new();
    private readonly HashSet<string> _deletedDirectories = new();

    public bool DirectoryExists(string path)
    {
        if (_deletedDirectories.Contains(path))
            return false;
        
        return _directoryExists.TryGetValue(path, out var exists) ? exists : false;
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        if (!DirectoryExists(path) || _deletedDirectories.Contains(path))
            return Enumerable.Empty<string>();

        var files = _directoryFiles.TryGetValue(path, out var directFiles) ? directFiles : new List<string>();
        
        if (searchOption == SearchOption.TopDirectoryOnly)
        {
            return files.Where(f => MatchesPattern(Path.GetFileName(f), searchPattern));
        }

        // For AllDirectories, include files from subdirectories
        var allFiles = new List<string>(files);
        foreach (var subDir in _directorySubdirectories.TryGetValue(path, out var subdirs) ? subdirs : new List<DirectoryInfo>())
        {
            if (!_deletedDirectories.Contains(subDir.FullName))
            {
                allFiles.AddRange(EnumerateFiles(subDir.FullName, searchPattern, SearchOption.AllDirectories));
            }
        }

        return allFiles.Where(f => MatchesPattern(Path.GetFileName(f), searchPattern));
    }

    public IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        if (!DirectoryExists(path) || _deletedDirectories.Contains(path))
            return Enumerable.Empty<DirectoryInfo>();

        var directories = _directorySubdirectories.TryGetValue(path, out var dirs) ? dirs : new List<DirectoryInfo>();
        
        var filteredDirs = directories.Where(d => !_deletedDirectories.Contains(d.FullName) && 
                                                  MatchesPattern(d.Name, searchPattern));

        if (searchOption == SearchOption.TopDirectoryOnly)
        {
            return filteredDirs;
        }

        // For AllDirectories, include subdirectories recursively
        var allDirs = new List<DirectoryInfo>(filteredDirs);
        foreach (var subDir in directories.Where(d => !_deletedDirectories.Contains(d.FullName)))
        {
            allDirs.AddRange(EnumerateDirectories(subDir.FullName, searchPattern, SearchOption.AllDirectories));
        }

        return allDirs;
    }

    public DateTime GetFileCreationTimeUtc(string filePath)
    {
        return _fileCreationTimes.TryGetValue(filePath, out var time) ? time : DateTime.UtcNow;
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        _deletedDirectories.Add(path);
        
        if (recursive && _directorySubdirectories.TryGetValue(path, out var subdirs))
        {
            foreach (var subdir in subdirs)
            {
                DeleteDirectory(subdir.FullName, true);
            }
        }
    }

    public async Task<Album?> DeserializeAlbumAsync(string filePath, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken); // Simulate async operation
        return _albumCache.TryGetValue(filePath, out var album) ? album : null;
    }

    public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;

    public string GetFileName(string path) => Path.GetFileName(path);

    public string CombinePath(params string[] paths) => Path.Combine(paths);

    // Test setup methods
    public MockFileSystemService SetDirectoryExists(string path, bool exists = true)
    {
        _directoryExists[path] = exists;
        return this;
    }

    public MockFileSystemService AddFilesToDirectory(string directoryPath, params string[] files)
    {
        if (!_directoryFiles.ContainsKey(directoryPath))
        {
            _directoryFiles[directoryPath] = new List<string>();
        }
        
        _directoryFiles[directoryPath].AddRange(files);
        return this;
    }

    public MockFileSystemService AddSubdirectory(string parentPath, string subdirectoryName)
    {
        if (!_directorySubdirectories.ContainsKey(parentPath))
        {
            _directorySubdirectories[parentPath] = new List<DirectoryInfo>();
        }

        var subdirPath = Path.Combine(parentPath, subdirectoryName);
        _directorySubdirectories[parentPath].Add(new DirectoryInfo(subdirPath));
        SetDirectoryExists(subdirPath);
        return this;
    }

    public MockFileSystemService SetFileCreationTime(string filePath, DateTime creationTime)
    {
        _fileCreationTimes[filePath] = creationTime;
        return this;
    }

    public MockFileSystemService SetAlbumForFile(string filePath, Album album)
    {
        _albumCache[filePath] = album;
        return this;
    }

    public MockFileSystemService Reset()
    {
        _directoryExists.Clear();
        _directoryFiles.Clear();
        _directorySubdirectories.Clear();
        _fileCreationTimes.Clear();
        _albumCache.Clear();
        _deletedDirectories.Clear();
        return this;
    }

    private static bool MatchesPattern(string name, string pattern)
    {
        if (pattern == "*" || pattern == "*.*")
            return true;

        // Simple pattern matching - can be enhanced for more complex patterns
        if (pattern.Contains("*"))
        {
            var parts = pattern.Split('*');
            if (parts.Length == 2)
            {
                return name.StartsWith(parts[0]) && name.EndsWith(parts[1]);
            }
        }

        return name.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
