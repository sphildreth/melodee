namespace Melodee.Common.Enums;

[Flags]
public enum ReleaseNeedsAttentionReasons
{
    NotSet = 0,

    HasMultipleArtistsButNotMultipleAristAlbumType = 1,

    HasNoMedia = 1 << 1,

    HasNoTracks = 1 << 2,

    IsMissingTracks = 1 << 3,

    TracksAreNotSequentiallyNumbered = 1 << 4,

    IsDuplicate = 1 << 5,

    HasDuplicateTracks = 1 << 6,

    HasTracksWithFeaturingFragments = 1 << 7,

    TitleHasUnwantedText = 1 << 8
}
