namespace Melodee.Blazor.ViewModels;

public class ArtistEdit
{
    public required string Name { get; set; }
    
    public required string NameNormalized { get; set; }
    
    public required string SortName { get; set; }
    
    public required int SearchEngineResultUniqueId { get; set; }
    
    public string? SpotifyId { get; set; }
    
    public string? MusicBrainzId { get; set; }
}
