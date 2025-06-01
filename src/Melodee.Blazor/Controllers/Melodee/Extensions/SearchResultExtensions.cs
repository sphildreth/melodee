using Melodee.Blazor.Controllers.Melodee.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class SearchResultExtensions
{
    public static SearchResult ToSearchResultModel(this global::Melodee.Common.Models.Search.SearchResult searchResult, string baseUrl, User currentUser, string userSecret)
    {
        return new SearchResult(
            searchResult.TotalCount,
            searchResult.Artists.Select(x => x.ToArtistModel(baseUrl, currentUser)).ToArray(),
            searchResult.TotalArtists,
            searchResult.Albums.Select(x => x.ToAlbumModel(baseUrl, currentUser)).ToArray(),
            searchResult.TotalAlbums,
            searchResult.Songs.Select(x => x.ToSongModel(baseUrl, currentUser, userSecret)).ToArray(),
            searchResult.TotalSongs,
            searchResult.Playlists.Select(x => x.ToPlaylistModel(baseUrl, currentUser)).ToArray(),
            searchResult.TotalPlaylists           
            );
    }
}
