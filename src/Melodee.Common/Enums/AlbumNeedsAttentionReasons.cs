namespace Melodee.Common.Enums;

[Flags]
public enum AlbumNeedsAttentionReasons
{
    NotSet = 0,

    /// <summary>
    ///     Unable to load album from file.
    /// </summary>
    AlbumCannotBeLoaded = 1 << 0,

    /// <summary>
    ///     Artist value is not set.
    /// </summary>
    ArtistIsNotSet = 1 << 1,

    /// <summary>
    ///     Artist is set, but has unwanted text.
    /// </summary>
    ArtistNameHasUnwantedText = 1 << 2,

    /// <summary>
    ///     Artist value is set (not null), but value is invalid.
    /// </summary>
    HasInvalidArtists = 1 << 3,

    /// <summary>
    ///     One of more of the songs on the album are invalid.
    /// </summary>
    HasInvalidSongs = 1 << 4,

    /// <summary>
    ///     The album year failed validation based on configuration.
    /// </summary>
    HasInvalidYear = 1 << 5,

    /// <summary>
    ///     Songs have different artists than the album artist but the album isn't to be a multiple artist type album (like VA
    ///     or OST).
    /// </summary>
    HasMultipleArtistsButNotMultipleAristAlbumType = 1 << 6,

    /// <summary>
    ///     Album has no images.
    /// </summary>
    HasNoImages = 1 << 7,

    /// <summary>
    ///     Album has no songs.
    /// </summary>
    HasNoSongs = 1 << 8,

    /// <summary>
    ///     Album has no tags.
    /// </summary>
    HasNoTags = 1 << 9,

    /// <summary>
    ///     One or more songs have invalid song number.
    /// </summary>
    HasSongsWithInvalidNumber = 1 << 10,

    /// <summary>
    ///     One or more songs has a song number that fails validation based on configuration.
    /// </summary>
    HasSongsWithNumberGreaterThanMaximumAllowed = 1 << 11,

    /// <summary>
    ///     One or more songs has unwanted text in the song title.
    /// </summary>
    HasSongsWithUnwantedText = 1 << 12,

    /// <summary>
    ///     Artist value is set but is unknown (no MusicBrainzId set)
    /// </summary>
    HasUnknownArtist = 1 << 14,

    /// <summary>
    ///     Is not a studio album (like live or broadcast).
    /// </summary>
    IsNotStudioTypeAlbum = 1 << 15,

    /// <summary>
    ///     The number of album songs don't match the total number of songs set in the albums tag.
    /// </summary>
    SongTotalDoesntMatchSongCount = 1 << 18,

    /// <summary>
    ///     Album has songs that isn't numbered sequentially (e.g. 1,2,7).
    /// </summary>
    SongsAreNotSequentiallyNumbered = 1 << 19,

    /// <summary>
    ///     Album has songs that have song numbers that are duplicated across album songs. (e.g. 1,1,2).
    /// </summary>
    SongsAreNotUniquelyNumbered = 1 << 20,

    /// <summary>
    ///     Album has title that includes unwanted text.
    /// </summary>
    TitleHasUnwantedText = 1 << 21,

    /// <summary>
    ///     Album title is not set or fails validation based on configuration.
    /// </summary>
    TitleIsInvalid = 1 << 22,

    /// <summary>
    ///     The first song should be numbered 1.
    /// </summary>
    HasInvalidFirstSongNumber = 1 << 23
}
