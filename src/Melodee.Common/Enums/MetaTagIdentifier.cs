namespace Melodee.Common.Enums;

/// <summary>
/// Meta Tag Identifier based on ID3 v2.4 
/// </summary>
public enum MetaTagIdentifier
{
    NotSet = 0,
    
    /// <summary>
    /// General description
    /// </summary>
    GeneralDescription,

    /// <summary>
    /// Title
    /// </summary>
    Title,

    /// <summary>
    /// Artist
    /// </summary>
    Artist,

    /// <summary>
    /// Composer
    /// </summary>
    Composer,

    /// <summary>
    /// Comment
    /// </summary>
    Comment,

    /// <summary>
    /// Genre
    /// </summary>
    Genre,

    /// <summary>
    /// Album
    /// </summary>
    Album,

    /// <summary>
    /// Recording year (when target format only supports year)
    /// </summary>
    RecordingYear,

    /// <summary>
    /// Recording date (when target format supports date)
    /// </summary>
    RecordingDate,

    /// <summary>
    /// Alternate to RECORDING_YEAR and RECORDING_DATE where the field may contain both
    /// </summary>
    RecordingDateOrYear,

    /// <summary>
    /// Recording day and month
    /// </summary>
    RecordingDaymonth,

    /// <summary>
    /// Recoding time
    /// </summary>
    RecordingTime,

    /// <summary>
    /// Track number
    /// </summary>
    TrackNumber,

    /// <summary>
    /// Disc number
    /// </summary>
    DiscNumber,

    /// <summary>
    /// Popularity (rating)
    /// </summary>
    Rating,

    /// <summary>
    /// Original artist
    /// </summary>
    OriginalArtist,

    /// <summary>
    /// Original album
    /// </summary>
    OriginalAlbum,

    /// <summary>
    /// Copyright
    /// </summary>
    Copyright,

    /// <summary>
    /// Album artist
    /// </summary>
    AlbumArtist,

    /// <summary>
    /// Publisher
    /// </summary>
    Publisher,

    /// <summary>
    /// Conductor
    /// </summary>
    Conductor,

    /// <summary>
    /// Total number of tracks
    /// </summary>
    TrackTotal,

    /// <summary>
    /// Alternate to TRACK_NUMBER and TRACK_TOTAL where both are in the same field
    /// </summary>
    TrackNumberTotal,

    /// <summary>
    /// Total number of discs
    /// </summary>
    DiscTotal,

    /// <summary>
    /// Alternate to DISC_NUMBER and DISC_TOTAL where both are in the same field
    /// </summary>
    DiscNumberTotal,

    /// <summary>
    /// Chapters table of contents description
    /// </summary>
    ChaptersTocDescription,

    /// <summary>
    /// Unsynchronized lyrics
    /// </summary>
    LyricsUnsynch,

    /// <summary>
    /// Publishing date
    /// </summary>
    PublishingDate,

    /// <summary>
    /// Product ID
    /// </summary>
    ProductId,

    /// <summary>
    /// Album sort order
    /// </summary>
    SortAlbum,

    /// <summary>
    /// Album artist sort order
    /// </summary>
    SortAlbumArtist,

    /// <summary>
    /// Artist sort order
    /// </summary>
    SortArtist,

    /// <summary>
    /// Title sort order
    /// </summary>
    SortTitle,

    /// <summary>
    /// Content group description
    /// </summary>
    Group,

    /// <summary>
    /// Series title / Movement name
    /// </summary>
    SeriesTitle,

    /// <summary>
    /// Series part / Movement index
    /// </summary>
    SeriesPart,

    /// <summary>
    /// Long description
    /// </summary>
    LongDescription,

    /// <summary>
    /// Beats per minute
    /// </summary>
    Bpm,

    /// <summary>
    /// Person or organization that encoded the file
    /// </summary>
    EncodedBy,

    /// <summary>
    /// Original release year (when target format only supports year)
    /// </summary>
    OrigReleaseYear,

    /// <summary>
    /// Original release date (when target format supports date)
    /// </summary>
    OrigReleaseDate,

    /// <summary>
    /// Software that encoded the file, with relevant settings if any
    /// </summary>
    Encoder,

    /// <summary>
    /// Language
    /// </summary>
    Language,

    /// <summary>
    /// International Standard Recording Code (ISRC)
    /// </summary>
    Isrc,

    /// <summary>
    /// Catalog number
    /// </summary>
    CatalogNumber,

    /// <summary>
    /// Audio source URL
    /// </summary>
    AudioSourceUrl,

    /// <summary>
    /// Lyricist
    /// </summary>
    Lyricist,

    /// <summary>
    /// Mapping between functions (e.g. "producer") and names. Every odd field is a
    /// function and every even is an name or a comma delimited list of names
    /// </summary>
    InvolvedPeople = 47
}