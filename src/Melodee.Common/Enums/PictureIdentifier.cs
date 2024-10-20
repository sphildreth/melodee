namespace Melodee.Common.Enums;

public enum PictureIdentifier
{
    NotSet = 0,

    /// <summary>
    ///     Unsupported (i.e. none of the supported values in the enum)
    /// </summary>
    Unsupported,

    /// <summary>
    ///     Generic
    /// </summary>
    Generic,

    /// <summary>
    ///     Front cover
    /// </summary>
    Front,

    /// <summary>
    ///     Front cover, but not the primary like Cover01, Cover02
    /// </summary>
    SecondaryFront,

    /// <summary>
    ///     Back cover
    /// </summary>
    Back,

    /// <summary>
    ///     Media (e.g. label side of CD)
    /// </summary>
    Cd,

    /// <summary>
    ///     File icon
    /// </summary>
    Icon,

    /// <summary>
    ///     Leaflet
    /// </summary>
    Leaflet,

    /// <summary>
    ///     Lead artist/lead performer/soloist
    /// </summary>
    LeadArtist,

    /// <summary>
    ///     Artist/performer
    /// </summary>
    Artist,

    /// <summary>
    ///     Artist/performer, but not the primary like Artist01, Artist02
    /// </summary>
    ArtistSecondary,

    /// <summary>
    ///     Conductor
    /// </summary>
    Conductor,

    /// <summary>
    ///     Band/Orchestra
    /// </summary>
    Band,

    /// <summary>
    ///     Band/Orchestra, not the primary image but like Band01, Band02
    /// </summary>
    BandSecondary,

    /// <summary>
    ///     Composer
    /// </summary>
    Composer,

    /// <summary>
    ///     Lyricist/text writer
    /// </summary>
    Lyricist,

    /// <summary>
    ///     Recording location
    /// </summary>
    RecordingLocation,

    /// <summary>
    ///     During recording
    /// </summary>
    DuringRecording,

    /// <summary>
    ///     During performance
    /// </summary>
    DuringPerformance,

    /// <summary>
    ///     Band/artist logotype
    /// </summary>
    BandLogo,

    /// <summary>
    ///     Publisher/Studio logotype
    /// </summary>
    PublisherLogo,

    /// <summary>
    ///     Label logotype
    /// </summary>
    LabelLogo,

    /// <summary>
    ///     Song image (usually in a compilation or a single)
    /// </summary>
    Song,

    /// <summary>
    ///     Song image, but not the primary like Song01_01, Song01_02
    /// </summary>
    SongSecondary
}
