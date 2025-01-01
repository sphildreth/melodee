namespace Melodee.Blazor.ViewModels;

public sealed record FileEdit
{
    public required FileSystemInfo FileSystemInfo { get; init; }
    
    public required string FileType { get; init; }
}
