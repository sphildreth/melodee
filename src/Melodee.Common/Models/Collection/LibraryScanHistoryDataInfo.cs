using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record LibraryScanHistoryDataInfo(
    int Id,
    Instant CreatedAt,
    string? ForArtistName,
    Guid? ForArtistApiKey,
    string? ForAlbumName,
    Guid? ForAlbumApiKey,
    int FoundArtistsCount,
    int FoundAlbumsCount,
    int FoundSongsCount,
    double DurationInMs
)
{
    public Duration Duration => Duration.FromMilliseconds(DurationInMs);
}
