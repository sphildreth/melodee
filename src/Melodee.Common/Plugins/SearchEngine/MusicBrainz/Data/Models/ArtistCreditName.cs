namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ArtistCreditName
{
    public long Id { get; init; }

    public bool IsValid => ArtistCreditId > 0 && ArtistId > 0;

    public long ArtistCreditId { get; init; }

    public int Position { get; init; }

    public long ArtistId { get; init; }

    public required string Name { get; init; }
}
