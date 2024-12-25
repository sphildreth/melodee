using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SearchEngines;

public sealed record ImageSearchResult
{
    public required string FromPlugin { get; init; }
    
    public short Rank { get; init; }
    
    public bool DoDeleteExistingCoverImages { get; set; }

    public long UniqueId { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public required string ThumbnailUrl { get; init; }

    public required string MediaUrl { get; init; }

    public string UrlValue => ThumbnailUrl.Nullify() ?? MediaUrl;

    public string? Title { get; init; }
}
