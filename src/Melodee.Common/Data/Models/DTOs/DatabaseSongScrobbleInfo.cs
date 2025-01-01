using NodaTime;

namespace Melodee.Common.Data.Models.DTOs;

public record DatabaseSongScrobbleInfo(
    Guid SongApiKey,
    int ArtistId,
    int AlbumId,
    int SongId,
    string ArtistName,
    string AlbumTitle,
    Instant TimePlayed,
    string SongTitle,
    double SongDuration,
    Guid? SongMusicBrainzId,
    int SongNumber
);
