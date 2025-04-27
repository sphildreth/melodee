namespace Melodee.Blazor.Components.Components;

public record ImageSearchResult(string ThumbnailUrl, string Url, string Title, bool DoDeleteAllOtherArtistImages, byte[]? ImageBytes = null);
