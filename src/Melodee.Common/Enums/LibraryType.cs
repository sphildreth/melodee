namespace Melodee.Common.Enums;

public enum LibraryType
{
    NotSet = 0,

    /// <summary>
    ///     Inbound holds metadata to get processed, should be 1.
    /// </summary>
    Inbound,

    /// <summary>
    ///     Processed metadata into metadata albums, should be 1
    /// </summary>
    Staging,

    /// <summary>
    ///     Storage library used by API, can be many
    /// </summary>
    Storage,

    /// <summary>
    ///     User images library, should be 1
    /// </summary>
    UserImages,
    
    /// <summary>
    ///     Holds data related to playlists including smart definitions and images, should be 1
    /// </summary>
    Playlist
}
