namespace Melodee.Common.Models.Search;

[Flags]
public enum SearchInclude
{
    NotSet = 0,
    Albums = 1 << 0,
    Artists = 1 << 1,
    Genres = 1 << 2,
    Playlists = 1 << 4,
    Songs = 1 << 5,
    
    Everything = Albums | Artists | Genres | Playlists | Songs
}
