namespace Melodee.Common.Models.Configuration;

public sealed record MagicOptions
{
    public bool IsMagicEnabled { get; init; } = true;
    
    public bool DoRenumberTracks { get; set; } = true;
    
    public bool DoRemoveFeaturingArtistFromTracksArtist { get; set; } = true;
    
    public bool DoRemoveFeaturingArtistFromTrackTitle { get; set; } = true;
    
    public bool DoReplaceTracksArtistSeperators { get; set; } = true;
    
    public bool DoSetYearToCurrentIfInvalid { get; set; } = true;
    
    public bool DoRemoveUnwantedTextFromReleaseTitle { get; set; } = true;
    
}
