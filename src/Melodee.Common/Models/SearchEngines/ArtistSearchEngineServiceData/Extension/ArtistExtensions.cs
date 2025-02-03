namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Extension;

public static class ArtistExtensions
{
    public static ArtistSearchResult ToArtistSearchResult(this Artist artist, string fromPlugin)
    {
        var result = new ArtistSearchResult
        {
            AmgId = artist.AmgId,
            DiscogsId = artist.DiscogsId,
            FromPlugin = fromPlugin,
            ItunesId = artist.ItunesId,
            LastFmId = artist.LastFmId,
            MusicBrainzId = artist.MusicBrainzId,
            Name = artist.Name,
            Rank = artist.Rank,
            SortName = artist.SortName,
            SpotifyId = artist.SpotifyId,
            UniqueId = artist.Id,
            WikiDataId = artist.WikiDataId
        };
        result.Releases = artist.Albums?.Select(x => x.ToAlbumSearchResult(artist, fromPlugin, result)).ToArray();
        return result;
    }
}
