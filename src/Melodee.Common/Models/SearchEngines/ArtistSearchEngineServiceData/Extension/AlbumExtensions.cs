namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Extension;

public static class AlbumExtensions
{
    public static AlbumSearchResult ToAlbumSearchResult(this Album album)
    {
        return new AlbumSearchResult
        {
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
