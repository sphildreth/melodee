namespace Melodee.Blazor.Controllers.Melodee.Models;

public record SearchResult(
    int TotalCount,
    Artist[] Artists,
    int TotalArtists,
    Album[] Albums,
    int TotalAlbums,
    Song[] Songs,
    int TotalSongs,
    Playlist[] Playlists,
    int TotalPlaylists);
