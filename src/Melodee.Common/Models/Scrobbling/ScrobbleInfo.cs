namespace Melodee.Common.Models.Scrobbling;

/// <summary>
/// Scrobble Info
/// </summary>
/// <param name="SongApiKey">The ApiKey of the Song to scrobble.</param>
/// <param name="SongTitle"> The song name.</param> 
/// <param name="ArtistName">The album artist name</param>
/// <param name="IsRandomizedScrobble">Flag if the user selected the song or played via some random operation.</param>
/// <param name="AlbumTitle">The album name</param>
/// <param name="SongDuration">The length of the track in seconds.</param>
/// <param name="SongMusicBrainzId">The MusicBrainz Track ID</param>
/// <param name="SongNumber">The song number of the song on the album.</param>
/// <param name="SongArtist">The album artist - if this differs from the song artist.</param>
public record ScrobbleInfo(
    Guid SongApiKey,
    string SongTitle,    
    string ArtistName,
    bool IsRandomizedScrobble,    
    string? AlbumTitle, 
    int? SongDuration,
    string? SongMusicBrainzId,
    int? SongNumber,
    string? SongArtist
    );