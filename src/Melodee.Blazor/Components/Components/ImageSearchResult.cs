namespace Melodee.Blazor.Components.Components;

public record ImageSearchResult(string ThumbnailUrl, string Url, string Title, bool DoDeleteAllOtherArtistImages, byte[]? ImageBytes = null)
{
    public string ImageBase64 => $"data:image/gif;base64,{Convert.ToBase64String(ImageBytes ?? [])}";
}
