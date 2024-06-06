namespace Melodee.Common.Models;

public sealed record DirectoryInfo
{
    public required string Path { get; init; }
    
    public int TotalItemsFound { get; init; }
    
    public int MusicFilesFound { get; init; }

}