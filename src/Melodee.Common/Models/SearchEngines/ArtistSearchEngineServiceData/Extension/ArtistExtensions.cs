namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Extension;

public static class ArtistExtensions
{
    public static ArtistSearchResult ToArtistSearchResult(this Artist artist, string fromPlugin)
    {
        return new ArtistSearchResult
        {
            AlbumCount = artist.Albums.Count,
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
            WikiDataId = artist.WikiDataId,
        };
    }
}
