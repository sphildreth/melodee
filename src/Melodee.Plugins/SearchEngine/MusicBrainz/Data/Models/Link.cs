namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record Link
{
    public long Id { get; init; }
    
    public long LinkTypeId { get; init; }
    
    public int BeginDateYear { get; init; }
    
    public int BeginDateMonth { get; init; }
    
    public int BeginDateDay { get; init; }
    
    public int EndDateYear { get; init; }
    
    public int EndDateMonth { get; init; }
    
    public int EndDateDay { get; init; }
    
    public bool IsEnded {get; init; }
}
