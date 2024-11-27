using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public record ArtistRelease
{
    [Ignore] public bool IsValid => ArtistId > 0 && ReleaseId > 0;

    [Index(Unique = false)] public long ArtistId { get; init; }

    [Index(Unique = false)] public required Guid ArtistMusicBrainzId { get; init; }

    [Index(Unique = false)] public long ReleaseId { get; init; }

    [Index(Unique = false)] public required Guid ReleaseMusicBrainzId { get; init; }

    public int FirstReleaseDate { get; init; }
}
