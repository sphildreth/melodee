using Melodee.Common.Models.Collection;

namespace Melodee.Common.Models.Search;

public sealed record SearchResult(
    ArtistDataInfo[] Artists,
    int TotalArtists,
    AlbumDataInfo[] Albums,
    int TotalAlbums,
    SongDataInfo[] Songs,
    int TotalSongs,
    PlaylistDataInfo[] Playlists,
    int TotalPlaylists,
    ArtistDataInfo[] MusicBrainzArtists,
    int TotalMusicBrainzArtists)
{
    public int TotalCount => TotalArtists + TotalAlbums + TotalSongs + TotalPlaylists + TotalMusicBrainzArtists;
}
