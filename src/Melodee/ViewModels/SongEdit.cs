namespace Melodee.ViewModels;

public sealed class SongEdit
{
    public bool IsSelected { get; set; }
    
    public int SongNumber { get; set; }
    
    public string? Title { get; set; }
    
    public string? Duration { get; init; }
}
