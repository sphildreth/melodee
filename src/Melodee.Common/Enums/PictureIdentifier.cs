namespace Melodee.Common.Enums;

public enum PictureIdentifier
{
    NotSet = 0,
    
    /// <summary>
    /// Unsupported (i.e. none of the supported values in the enum)
    /// </summary>
    Unsupported,

    /// <summary>
    /// Generic
    /// </summary>
    Generic,

    /// <summary>
    /// Front cover
    /// </summary>
    Front,

    /// <summary>
    /// Back cover
    /// </summary>
    Back,

    /// <summary>
    /// Media (e.g. label side of CD)
    /// </summary>
    Cd,

    /// <summary>
    /// File icon
    /// </summary>
    Icon,

    /// <summary>
    /// Leaflet
    /// </summary>
    Leaflet,

    /// <summary>
    /// Lead artist/lead performer/soloist
    /// </summary>
    LeadArtist,

    /// <summary>
    /// Artist/performer
    /// </summary>
    Artist,

    /// <summary>
    /// Conductor
    /// </summary>
    Conductor,

    /// <summary>
    /// Band/Orchestra
    /// </summary>
    Band,

    /// <summary>
    /// Composer
    /// </summary>
    Composer,

    /// <summary>
    /// Lyricist/text writer
    /// </summary>
    Lyricist,

    /// <summary>
    /// Recording location
    /// </summary>
    RecordingLocation,

    /// <summary>
    /// During recording
    /// </summary>
    DuringRecording,

    /// <summary>
    /// During performance
    /// </summary>
    DuringPerformance,

    /// <summary>
    /// Band/artist logotype
    /// </summary>
    BandLogo,

    /// <summary>
    /// Publisher/Studio logotype
    /// </summary>
    PublisherLogo = 20
}