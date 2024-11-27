namespace Melodee.Common.Models.SearchEngines;

public sealed record ImageSearchResult
{
    public bool DoDeleteExistingCoverImages { get; set; }

    public long UniqueId { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public required string ThumbnailUrl { get; init; }

    public required string MediaUrl { get; init; }

    public string? Title { get; init; }
}
