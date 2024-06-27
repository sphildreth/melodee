using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a "file" directory.
/// </summary>
[Serializable]
public sealed record FileSystemDirectoryInfo
{
    public long UniqueId => SafeParser.Hash(Path);

    public bool ShowInTree => MusicFilesFound > 0;
    
    public long ParentId { get; init; }
    
    /// <summary>
    /// Full path to Directory
    /// </summary>
    public required string Path { get; init; }
    
    public required string Name { get; init; }
    
    public int TotalItemsFound { get; init; }
    
    public int MusicFilesFound { get; init; }
    
    public int MusicMetaDataFilesFound { get; init; }
    
    public int ImageFilesFound { get; init; }

    public override string ToString()
    {
        return $"UniqueId [{ UniqueId }] Path [{Path}] ShortName [{Name}]";
    }
}