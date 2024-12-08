namespace Melodee.Common.Models.SearchEngines;

public sealed record AlbumQuery : Query
{
    public required int Year { get; init; }
}
