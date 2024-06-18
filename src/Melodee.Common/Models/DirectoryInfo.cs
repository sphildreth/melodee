using System.Dynamic;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a "file" directory which usually contains release files or nested release directories.
/// </summary>
[Serializable]
public sealed record DirectoryInfo
{
    public long UniqueId => SafeParser.Hash($"{ParentId}{Path}"); 

    public long ParentId { get; init; }
    
    public required string Path { get; init; }
    
    public required string ShortName { get; init; }
    
    public int TotalItemsFound { get; init; }
    
    public int MusicFilesFound { get; init; }
    
    public int MusicMetaDataFilesFound { get; init; }
    
    public int ImageFilesFound { get; init; }

}