namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public record ArtistRelease
{
    public bool IsValid => ArtistId > 0 && ReleaseId > 0;

    public long ArtistId { get; init; }

    public required Guid ArtistMusicBrainzId { get; init; }

    public long ReleaseId { get; init; }

    public required Guid ReleaseMusicBrainzId { get; init; }

    public int FirstReleaseDate { get; init; }
}
