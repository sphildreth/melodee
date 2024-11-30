using NodaTime;

namespace Melodee.Common.Data.Models.DTOs;

public record DatabaseSongScrobbleInfo
(
    Guid SongApiKey,
    string ArtistName,
    string AlbumTitle,
    Instant TimePlayed,
    string SongTitle,
    double SongDuration,
    Guid? SongMusicBrainzId,
    int SongNumber
);
