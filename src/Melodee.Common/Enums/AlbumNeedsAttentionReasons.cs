namespace Melodee.Common.Enums;

[Flags]
public enum AlbumNeedsAttentionReasons
{
    NotSet = 0,
    AlbumCannotBeLoaded = 1,
    ArtistNameHasUnwantedText = 1 << 1,
    HasMultipleArtistsButNotMultipleAristAlbumType = 1 << 2,
    HasNoImages = 1 << 3,
    HasNoSongs = 1 << 4,
    HasSongsWithUnwantedText = 1 << 5,
    HasSongsWithoutMediaNumberSet = 1 << 6,
    HasUnknownArtist = 1 << 7,
    IsNotStudioTypeAlbum = 1 << 8,
    MediaNumbersAreInvalid = 1 << 9,
    MediaTotalNumberDoesntMatchMediaFound = 1 << 10,
    SongTotalDoesntMatchSongCount = 1 << 11,
    SongsAreNotSequentiallyNumbered = 1 << 12,
    SongsAreNotUniquelyNumbered = 1 << 13,
    TitleHasUnwantedText = 1 << 14,
    IsInvalid = 1 << 15,
    ArtistIsNotSet = 1 << 16,
    HasSongWithInvalidNumber = 1 << 17,
    HasSongWithNumberGreaterThanMaximumAllowed = 1 << 18,
}
