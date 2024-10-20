namespace Melodee.Common.Enums;

[Flags]
public enum AlbumNeedsAttentionReasons
{
    NotSet = 0,

    HasMultipleArtistsButNotMultipleAristAlbumType = 1,

    HasNoMedia = 1 << 1,

    HasNoSongs = 1 << 2,

    IsMissingSongs = 1 << 3,

    SongsAreNotSequentiallyNumbered = 1 << 4,

    IsDuplicate = 1 << 5,

    HasDuplicateSongs = 1 << 6,

    HasSongsWithFeaturingFragments = 1 << 7,

    TitleHasUnwantedText = 1 << 8
}
