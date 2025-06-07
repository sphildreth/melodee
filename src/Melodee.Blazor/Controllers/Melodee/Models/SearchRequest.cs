namespace Melodee.Blazor.Controllers.Melodee.Models;

public record SearchRequest(string Query, string? Type, short? AlbumPage, short? ArtistPage, short? SongPage, short? PageSize, string? SortBy, string? SortOrder)
{
    public short AlbumPageValue => AlbumPage ?? 1;
    
    public short ArtistPageValue => ArtistPage ?? 1;
    
    public short SongPageValue => SongPage ?? 1;
}
