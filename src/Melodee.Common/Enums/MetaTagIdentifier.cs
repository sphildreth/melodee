namespace Melodee.Common.Enums;

/// <summary>
/// Meta Tag Identifier based on ID3 v2.4 
/// </summary>
public enum MetaTagIdentifier
{
    NotSet = 0,
  
    /// <summary>
    /// General description [TT1]
    /// </summary>
    GeneralDescription,
    
    /// <summary>
    /// Track Artist [TP1,TPE1] versus Album Artist
    /// </summary>
    Artist,
    
    /// <summary>
    /// Doesn't look like an official ID3 tag but often used for multiple value artists on a track.
    /// </summary>
    [MetaTagMultiValue(true)]
    Artists,

    /// <summary>
    /// Composer [TCM, TCOM]
    /// </summary>
    [MetaTagMultiValue(true)]
    Composer,

    /// <summary>
    /// Comment [COM, COMM]
    /// </summary>
    Comment,

    /// <summary>
    /// Genre [TCO, TCON]
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

    /*
     * Recording date <== de facto standard behind the "date" field on most taggers
     * ID3v2.2 : TYE (year), TDA (day & month - DDMM), TIM (hour & minute - HHMM)
     * ID3v2.3 : TYER (year), TDAT (day & month - DDMM), TIME (hour & minute - HHMM)
     * NB : Some loose implementations actually use TDRC inside ID3v2.3 headers
     * ID3v2.4 : TDRC (timestamp)
     */    
    
    /// <summary>
    /// Recording year (when target format only supports year) [TYE,TYER]
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
    /// Recording day and month [TDA, TDAT]
    /// </summary>
    RecordingDayMonth,

    /// <summary>
    /// Recoding time [TIM, TIME]
    /// </summary>
    RecordingTime,

    /// <summary>
    /// Track number. This should be set from parsed value from [TRK] or [TRCK]
    /// </summary>
    TrackNumber,

    /// <summary>
    /// Popularity (rating) [POP, POPM]
    /// </summary>
    Rating,

    /// <summary>
    /// Original artist [TOA, TOPE]
    /// </summary>
    [MetaTagMultiValue(true)]
    OriginalArtist,
    
    /// <summary>
    /// Original album [TOT,TOAL]
    /// </summary>
    OriginalAlbum,

    /// <summary>
    /// Copyright [TCR, TCOP]
    /// </summary>
    Copyright,
    
    /// <summary>
    /// [WCP, WCOP]
    /// </summary>
    CopyrightUrl,

    /// <summary>
    /// The ‘Band/Orchestra/Accompaniment’ frame is used for additional information about the performers in the recording. [TP2, TPE2]
    /// </summary>
    AlbumArtist,

    /// <summary>
    /// Publisher [TPB, TPUB, LABEL]
    /// </summary>
    Publisher,
    
    /// <summary>
    /// [WPB, WPUB]
    /// </summary>
    PublisherUrl,

    /// <summary>
    /// Conductor [TP3,TPE3]
    /// </summary>
    Conductor,

    /// <summary>
    /// Total number of tracks [TRK]
    /// </summary>
    TrackTotal,

    /// <summary>
    /// Alternate to TRACK_NUMBER and TRACK_TOTAL where both are in the same field [TRCK] ('1/8')
    /// </summary>
    TrackNumberTotal,
    
    /// <summary>
    /// Disc number. This should be set from parsed value from [TPA] or [TPOS]
    /// </summary>
    DiscNumber,    

    /// <summary>
    /// Total number of discs [TPA]
    /// </summary>
    DiscTotal,

    /// <summary>
    /// Alternate to DiscNumber and DiscTotal where both are in the same field [TPOS] ('1/8')
    /// </summary>
    DiscNumberTotal,

    /// <summary>
    /// Chapters table of contents description [CTOC]
    /// </summary>
    ChaptersTocDescription,

    /// <summary>
    /// UnSynchronized lyrics [USLT]
    /// </summary>
    LyricsUnSynch,

    /// <summary>
    /// Product ID
    /// </summary>
    ProductId,

    /// <summary>
    /// Album sort order [TSOA]
    /// </summary>
    SortAlbum,

    /// <summary>
    /// Album artist sort order [TS02]
    /// </summary>
    SortAlbumArtist,

    /// <summary>
    /// Artist sort order [TSOP]
    /// </summary>
    SortArtist,

    /// <summary>
    /// Title sort order [TSOT]
    /// </summary>
    SortTitle,

    /// <summary>
    /// Content group description [TIT1]
    /// </summary>
    Group,

    /// <summary>
    /// MovementName [MVNM]
    /// </summary>
    MovementName,

    /// <summary>
    /// MovementNumber [MVIN]
    /// </summary>
    MovementNumber,
    
    /// <summary>
    /// Total parsed number of Movements, no official tag.
    /// </summary>
    MovementTotal,
    
    /// <summary>
    /// Alternate to MovementNumber and MovementTotal where both are in the same field [MVIN] ('1/5')
    /// </summary>
    MovementNumberTotal,

    /// <summary>
    /// Long description [TDES]
    /// </summary>
    LongDescription,

    /// <summary>
    /// Beats per minute [TBP, TBPM]
    /// </summary>
    Bpm,

    /// <summary>
    /// Person or organization that encoded the file [TEN, TENC]
    /// </summary>
    EncodedBy,


    /*
     *  Original release date
     *  ID3v2.2 : TOR (year only)
     *  ID3v2.3 : TORY (year only)
     *  ID3v2.4 : TDOR (timestamp according to spec)
    */
    
    /// <summary>
    /// Original release year (when target format only supports year) [TORY,TOR]
    /// </summary>
    OrigReleaseYear,
    
    /// <summary>
    /// The ‘Date’ frame is a numeric string in the DDMM format containing the date for the recording. This field is always four characters long. [TDA, TDAT]
    /// </summary>
    Date, 
    
    /*
     * Release date
     * ID3v2.2 : no standard
     * ID3v2.3 : no standard
     * ID3v2.4 : TDRL (timestamp according to spec; actual content may vary)
    */
    
    /// <summary>
    /// Original release date (when target format supports date) [TDOR]
    /// </summary>
    OrigReleaseDate,
    
    /// <summary>
    /// Release date. [TDRL]
    /// </summary>
    ReleaseDate,

    /// <summary>
    /// Software that encoded the file, with relevant settings if any [TSS, TSSE]
    /// </summary>
    Encoder,

    /// <summary>
    /// Language [TLA, TLAN]
    /// </summary>
    [MetaTagMultiValue(true)]
    Language,

    /// <summary>
    /// International Standard Recording Code [TRC, TSRC]
    /// </summary>
    Isrc,

    /// <summary>
    /// Catalog number [CATALOGNUMBER]
    /// </summary>
    CatalogNumber,

    /// <summary>
    /// Audio source URL [WAS, WOAS]
    /// </summary>
    AudioSourceUrl,

    /// <summary>
    /// Lyricist [TXT, TEXT]
    /// </summary>
    [MetaTagMultiValue(true)]
    Lyricist,

    /// <summary>
    /// Mapping between functions (e.g. "producer") and names. Every odd field is a function and every even is a name or a comma delimited list of names. [IPL, IPLS, TIPL]
    /// </summary>
    InvolvedPeople,
    
    /// <summary>
    /// Represents a user defined URL link ID3 frame. [WXX, WXXX]
    /// </summary>
    UserDefinedUrlLink,
    
    /// <summary>
    /// [WPAY]
    /// </summary>
    PaymentUrl,
    
    /// <summary>
    /// [WCM, WCOM]
    /// </summary>
    CommercialUrl,
    
    /// <summary>
    /// [USER]
    /// </summary>
    TermsOfUse,
    
    /// <summary>
    /// [CNT, PCNT]
    /// </summary>
    PlayCounter,
    
    /// <summary>
    /// [CRA, AENC]
    /// </summary>
    AudioEncryption,
    
    /// <summary>
    /// [BUF, RBUF]
    /// </summary>
    RecommendedBufferSize,
    
    /// <summary>
    /// [ETC, ETCO]
    /// </summary>
    EventTimingCodes,
    
    /// <summary>
    /// [EQU, EQU2]
    /// </summary>
    Equalisation,
    
    /// <summary>
    /// [GEO, GEOB]
    /// </summary>
    GeneralEnscapsulatedObject,
    
    /// <summary>
    /// [TLE, TLEN]
    /// </summary>
    Length,
  
    /// <summary>
    /// [LNK, LINK]
    /// </summary>
    LinkedInformation,
    
    /// <summary>
    /// [MCI, MCDI]
    /// </summary>
    MusicCdIdentifier,
    
    /// <summary>
    /// [MLL, MLLT]
    /// </summary>
    MpegLocationLookupTable,
    
    /// <summary>
    /// [REV, RVRB]
    /// </summary>
    Reverb,
    
    /// <summary>
    /// [RVA, RVA2]
    /// </summary>
    RelativeVolumeAdjustment,
    
    /// <summary>
    /// [STC, SYTC]
    /// </summary>
    SynchronizedTempoCodes,
    
    /// <summary>
    /// [TDY, TDLY]
    /// </summary>
    PlaylistDelay,
    
    /// <summary>
    /// [TFT, TFLT]
    /// </summary>
    FileType,
    
    /// <summary>
    /// [TKE, TKEY]
    /// </summary>
    InitialKey,
    
    /// <summary>
    /// [TMT, TMED]
    /// </summary>
    MediaType,
    
    /// <summary>
    /// [TOF, TOFN]
    /// </summary>
    OriginalFilename,
    
    /// <summary>
    /// [TOL, TOLY]
    /// </summary>
    [MetaTagMultiValue(true)]
    OriginalLyricist,
    
    /// <summary>
    /// [TP4, TPE4]
    /// </summary>
    InterpretedRemixedOrOtherwiseModifiedBy,
 
    /// <summary>
    /// [TXX, TXXX]
    /// </summary>
    UserDefinedTextInformation,
 
    /// <summary>
    /// [UFI, UFID]
    /// </summary>
    UniqueFileIdentifier,
    
    /// <summary>
    /// [ULT, USLT]
    /// </summary>
    UnsynchronisedLyrics,
    
    /// <summary>
    /// [WAF, WOAF]
    /// </summary>
    OfficialAudioFileWebpage,
    
    /// <summary>
    /// [WAR, WOAR]
    /// </summary>
    OfficialArtistPerformerWebpage,
    
    /// <summary>
    /// Not a ID3 standard, a unique ID given to an artist to prevent exact named artists getting the same library folder.
    /// </summary>
    UniqueArtistId,
    
    /// <summary>
    /// Used when parsing CUE Files.
    /// </summary>
    SubCodeFlags
}