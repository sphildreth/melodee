namespace Melodee.Common.Enums;

[Flags]
public enum AlbumNeedsAttentionReasons
{
    NotSet = 0,
    AlbumCannotBeLoaded = 1,
    ArtistIsNotSet = 1 << 2,
    ArtistNameHasUnwantedText = 1 << 3,
    HasInvalidArtists = 1 << 4,
    HasInvalidSongs = 1 << 5,
    HasInvalidYear = 1 << 6,
    HasMultipleArtistsButNotMultipleAristAlbumType = 1 << 7,
    HasNoImages = 1 << 8,
    HasNoSongs = 1 << 9,
    HasNoTags = 1 << 10,
    HasSongWithInvalidNumber = 1 << 11,
    HasSongWithNumberGreaterThanMaximumAllowed = 1 << 12,
    HasSongsWithUnwantedText = 1 << 13,
    HasSongsWithoutMediaNumberSet = 1 << 14,
    HasUnknownArtist = 1 << 15,
    IsInvalid = 1 << 16,
    IsNotStudioTypeAlbum = 1 << 17,
    MediaNumbersAreInvalid = 1 << 18,
    MediaTotalNumberDoesntMatchMediaFound = 1 << 19,
    SongTotalDoesntMatchSongCount = 1 << 20,
    SongsAreNotSequentiallyNumbered = 1 << 21,
    SongsAreNotUniquelyNumbered = 1 << 22,
    TitleHasUnwantedText = 1 << 23,
    TitleIsInvalid = 1 << 24
}
