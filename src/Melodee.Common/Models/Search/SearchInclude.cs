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
    MusicBrainz = 1 << 6,
    Users = 1 << 7,
    RadioStations = 1 << 8,
    Shares = 1 << 9,
    Contributors = 1 << 10,
    
    Data = Albums | Artists | Songs,
    
    MetaData = MusicBrainz,
    
    Everything = Albums | Artists | Genres | Playlists | Songs | MusicBrainz | Users | RadioStations | Shares | Contributors
}
