namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record LinkArtistToArtist
{
    public long Id { get; init; }
    
    public required long LinkId { get; init; }
    
    public required  long Artist0 { get; init; }
    
    public required  long Artist1 { get; init; }
    
    public required int LinkOrder { get; init; }
    
    public required string Artist0Credit{ get; init; }
    
    public required string Artist1Credit{ get; init; }
}
