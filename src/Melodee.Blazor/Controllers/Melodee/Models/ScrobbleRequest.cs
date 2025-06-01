using Melodee.Common.Utility;

namespace Melodee.Blazor.Controllers.Melodee.Models;

/// <summary>
/// Details sent when an API application sends a request to create a scrobble for a user for a song.
/// </summary>
/// <param name="SongId">ApiKey of the Song being scrobbled.</param>
/// <param name="PlayerName">The application the user is using to play the song and do the scrobble request.</param>
/// <param name="ScrobbleType">Type of scrobble request</param>
/// <param name="Timestamp">The timestamp of when the scrobble request was made.</param>
/// <param name="PlayedDuration">The amount of time the song has been played. This is likely null on not Played scrobble request.</param>
public record ScrobbleRequest(
    Guid SongId,
    string PlayerName,
    string ScrobbleType,
    double? Timestamp,
    double? PlayedDuration)
{
    public ScrobbleRequestType ScrobbleTypeValue => SafeParser.ToEnum<ScrobbleRequestType>(ScrobbleType);
}

public enum ScrobbleRequestType
{
    NotSet = 0,
    
    NowPlaying,
    
    Played
}
