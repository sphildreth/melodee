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
    /// The ‘Album/Movie/Show title’ frame is intended for the title of the recording(/source of sound) which the audio in the file is taken from. [TALB]
    /// </summary>
    Album,
    
    /// <summary>
    /// The ‘Title/Songname/Content description’ frame is the actual name of the piece [TT2, TIT2]
    /// </summary>
    Title,
    
    /// <summary>
    /// The ‘Subtitle/Description refinement’ frame is used for information directly related to the contents title  [TT3, TIT3]
    /// </summary>
    SubTitle,

    /// <summary>
    /// Recording year (when target format only supports year)
    /// </summary>
    RecordingYear,

    /// <summary>
    /// Recording date (when target format supports date) [TRD, TRDA]
    /// </summary>
    RecordingDate,

    /// <summary>
    /// Alternate to RECORDING_YEAR and RECORDING_DATE where the field may contain both
    /// </summary>
    RecordingDateOrYear,

    /// <summary>
    /// Recording day and month
    /// </summary>
    RecordingDayMonth,

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
    /// he ‘Band/Orchestra/Accompaniment’ frame is used for additional information about the performers in the recording. [TP2, TPE2]
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
    /// Alternate to TRACK_NUMBER and TRACK_TOTAL where both are in the same field [TRCK] ('1/8')
    /// </summary>
    TrackNumberTotal,

    /// <summary>
    /// Total number of discs
    /// </summary>
    DiscTotal,

    /// <summary>
    /// Alternate to DISC_NUMBER and DISC_TOTAL where both are in the same field [TPOS] ('1/8')
    /// </summary>
    DiscNumberTotal,

    /// <summary>
    /// Chapters table of contents description
    /// </summary>
    ChaptersTocDescription,

    /// <summary>
    /// UnSynchronized lyrics
    /// </summary>
    LyricsUnSynch,

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
    /// MovementName [MVNM]
    /// </summary>
    SeriesTitle,

    /// <summary>
    /// MovementNumber [MVIN]
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
    /// Original release year (when target format only supports year) [TORY,TOR]
    /// </summary>
    OrigReleaseYear,
    
    /// <summary>
    /// The ‘Date’ frame is a numeric string in the DDMM format containing the date for the recording. This field is always four characters long. [TDA, TDAT]
    /// </summary>
    Date, 

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
    /// function and every even is a name or a comma delimited list of names
    /// </summary>
    InvolvedPeople
}