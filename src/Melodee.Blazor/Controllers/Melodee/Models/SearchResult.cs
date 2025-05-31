namespace Melodee.Blazor.Controllers.Melodee.Models;

public record SearchResult(int TotalCount, Artist[] Artists, Album[] Albums, Song[] Songs, Playlist[] Playlists);
