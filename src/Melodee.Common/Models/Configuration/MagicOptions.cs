namespace Melodee.Common.Models.Configuration;

public sealed record MagicOptions
{
    public bool IsMagicEnabled { get; init; } = true;
    
    public bool DoRenumberSongs { get; set; } = true;
    
    public bool DoRemoveFeaturingArtistFromSongsArtist { get; set; } = true;
    
    public bool DoRemoveFeaturingArtistFromSongTitle { get; set; } = true;
    
    public bool DoReplaceSongsArtistSeperators { get; set; } = true;
    
    public bool DoSetYearToCurrentIfInvalid { get; set; } = true;
    
    public bool DoRemoveUnwantedTextFromAlbumTitle { get; set; } = true;
    
}
