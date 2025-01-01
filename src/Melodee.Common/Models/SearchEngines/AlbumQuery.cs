namespace Melodee.Common.Models.SearchEngines;

public sealed record AlbumQuery : Query
{
    public string? Artist { get; set; }

    public string? ArtistMusicBrainzId { get; set; }

    public required int Year { get; init; }
}
