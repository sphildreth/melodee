namespace Melodee.Common.Models.SearchEngines;

public record ImageSearchResult(long UniqueId, int Width, int Height, string ThumbnailUrl, string MediaUrl, string Title)
{
    public bool DoDeleteExistingCoverImages { get; set; }
}
