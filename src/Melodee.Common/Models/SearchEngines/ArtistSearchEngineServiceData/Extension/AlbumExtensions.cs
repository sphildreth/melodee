namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Extension;

public static class AlbumExtensions
{
    public static AlbumSearchResult ToAlbumSearchResult(this Album album, Artist artist, string fromPlugin, ArtistSearchResult? artistSearchResult = null )
    {
        return new AlbumSearchResult
        {
            Artist = artistSearchResult ?? artist.ToArtistSearchResult(fromPlugin),
            UniqueId = album.Id,
            AlbumType = album.AlbumTypeValue,
            ReleaseDate = $"01/01/{ album.Year}",
            Name = album.Name,
            NameNormalized = album.NameNormalized,
            SortName = album.SortName ?? album.Name,
            MusicBrainzId = album.MusicBrainzId
        };
    }
}
