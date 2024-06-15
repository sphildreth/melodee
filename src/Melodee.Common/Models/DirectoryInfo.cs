using System.Dynamic;

namespace Melodee.Common.Models;

[Serializable]
public sealed record DirectoryInfo
{
    public int Id => Path.GetHashCode() + ShortName.GetHashCode();

    public int ParentId { get; init; }
    
    public required string Path { get; init; }
    
    public required string ShortName { get; init; }
    
    public int TotalItemsFound { get; init; }
    
    public int MusicFilesFound { get; init; }
    
    public int MusicMetaDataFilesFound { get; init; }
    
    public int ImageFilesFound { get; init; }

}