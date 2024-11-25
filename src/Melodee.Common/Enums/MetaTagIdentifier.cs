using System.ComponentModel;
using Melodee.Common.Attributes;

namespace Melodee.Common.Enums;

/// <summary>
///     Meta Tag Identifier based on ID3 v2.4
/// </summary>
public enum MetaTagIdentifier
{
    NotSet = 0,

    /// <summary>
    ///     General description
    /// </summary>
    [Description("[TT1]")] GeneralDescription,

    /// <summary>
    ///     The 'Lead performer(s)/Soloist(s)' tag. Song Artist [TP1,TPE1] versus Album Artist
    /// </summary>
    [Description("[TP1,TPE1]")] Artist,

    /// <summary>
    ///     Doesn't look like an official ID3 tag but often used for multiple value artists on a Song.
    /// </summary>
    [Description("[TT1]")] [MetaTagMultiValue(true)]
    Artists,

    /// <summary>
    ///     Composer [TCM, TCOM]
    /// </summary>
    [Description("[TCM, TCOM]")] [MetaTagMultiValue(true)]
    Composer,

    /// <summary>
    ///     Comment [COM, COMM]
    /// </summary>
    [Description("[COM, COMM]")] Comment,
    
    /// <summary>
    ///     Musician Credits [TMCL]
    /// </summary>
    [Description("TMCL")] MusicianCredit,

    /// <summary>
    ///     Genre [TCO, TCON]
    /// </summary>
    [Description("[TCO, TCON]")] Genre,

    /// <summary>
    ///     The ‘Album/Movie/Show title’ frame is intended for the title of the recording(/source of sound) which the audio in
    ///     the file is taken from. [TALB]
    /// </summary>
    [Description("[TALB]")] Album,

    /// <summary>
    /// Disc / Set Subtitle (for multi-disc sets) [TSST] 
    /// </summary>
    [Description("[TSST]")] DiscSetSubtitle,
    
    /// <summary>
    ///     The ‘Title/Songname/Content description’ frame is the actual name of the piece [TT2, TIT2]
    /// </summary>
    [Description("[TT2, TIT2]")] Title,

    /// <summary>
    ///     The ‘Subtitle/Description refinement’ frame is used for information directly related to the contents title  [TT3, TIT3]
    /// </summary>
    [Description("[TT3, TIT]")] SubTitle,

    /*
     * Recording date <== de facto standard behind the "date" field on most taggers
     * ID3v2.2 : TYE (year), TDA (day & month - DDMM), TIM (hour & minute - HHMM)
     * ID3v2.3 : TYER (year), TDAT (day & month - DDMM), TIME (hour & minute - HHMM)
     * NB : Some loose implementations actually use TDRC inside ID3v2.3 headers
     * ID3v2.4 : TDRC (timestamp)
     */

    /// <summary>
    ///     Recording year (when target format only supports year) [TYE,TYER]
    /// </summary>
    [Description("[TYE,TYER]")] RecordingYear,

    /// <summary>
    ///     Recording date (when target format supports date) [TRD, TRDA]
    /// </summary>
    [Description("[TRD, TRD]")] RecordingDate,

    /// <summary>
    ///     Alternate to RECORDING_YEAR and RECORDING_DATE where the field may contain both
    /// </summary>
    [Description("[RecordingDateOrYear]")] RecordingDateOrYear,

    /// <summary>
    ///     Recording day and month [TDA, TDAT]
    /// </summary>
    [Description("[TDA, TDAT]")] RecordingDayMonth,

    /// <summary>
    ///     Recoding time [TIM, TIME]
    /// </summary>
    [Description("[TIM, TIME]")] RecordingTime,

    /// <summary>
    ///     Song number. This should be set from parsed value from [TRK] or [TRCK]
    /// </summary>
    [Description("[TRK, TRCK]")] TrackNumber,

    /// <summary>
    ///     Popularity (rating) [POP, POPM]
    /// </summary>
    [Description("[POP, POPM]")] Rating,

    /// <summary>
    ///     Original artist [TOA, TOPE]
    /// </summary>
    [MetaTagMultiValue(true)] [Description("[TOA, TOPE]")]
    OriginalArtist,

    /// <summary>
    ///     Original album [TOT,TOAL]
    /// </summary>
    [Description("[TOT,TOAL]")] OriginalAlbum,

    /// <summary>
    ///     Copyright [TCR, TCOP]
    /// </summary>
    [Description("[TCR, TCOP]")] Copyright,

    /// <summary>
    ///     [WCP, WCOP]
    /// </summary>
    [Description("[WCP, WCOP]")] CopyrightUrl,

    /// <summary>
    ///     The ‘Band/Orchestra/Accompaniment’ frame is used for additional information about the performers in the recording.
    ///     [TP2, TPE2]
    /// </summary>
    [Description("[TP2, TPE2]")] AlbumArtist,

    /// <summary>
    ///     Publisher [TPB, TPUB, LABEL]
    /// </summary>
    [Description("[TPB, TPUB, LABEL]")] Publisher,

    /// <summary>
    ///     [WPB, WPUB]
    /// </summary>
    [Description("[WPB, WPUB]")] PublisherUrl,

    /// <summary>
    ///     Conductor [TP3,TPE3]
    /// </summary>
    [Description("[TP3,TPE3]")] Conductor,
    
    /// <summary>
    /// Engineer
    /// </summary>
    [Description("TIPL:ENGINEER")] Engineer,

    /// <summary>
    ///     Total number of Songs [TRK]
    /// </summary>
    [Description("[TRK]")] SongTotal,

    /// <summary>
    ///     Alternate to Song_NUMBER and Song_TOTAL where both are in the same field [TRCK] ('1/8')
    /// </summary>
    [Description("[TRCK]")] SongNumberTotal,

    /// <summary>
    ///     Disc number. This should be set from parsed value from [TPA] or [TPOS]
    ///     The ‘Part of a set’ [TPOS] frame is a numeric string that describes which part of a set the audio came from. This
    ///     frame is used if the source described in the “TALB” frame is divided into several mediums, e.g. a double CD.
    ///     If the audio is not part of a set the value is '0' (not '1' as '1' would indicate it's the first part of a set
    ///     versus '0' which means it is not part of a set.)
    /// </summary>
    [Description("[TPOS]")] DiscNumber,

    /// <summary>
    ///     Total number of discs [TPA]
    /// </summary>
    [Description("[TPA]")] DiscTotal,

    /// <summary>
    ///     Alternate to DiscNumber and DiscTotal where both are in the same field [TPOS] ('1/8')
    /// </summary>
    [Description("[TPOS]")] DiscNumberTotal,

    /// <summary>
    ///     Chapters table of contents description [CTOC]
    /// </summary>
    [Description("[CTOC]")] ChaptersTocDescription,

    /// <summary>
    ///     Product ID
    /// </summary>
    [Description("[ProductId]")] ProductId,

    /// <summary>
    ///     Album sort order [TSOA]
    /// </summary>
    [Description("[TSOA]")] SortAlbum,

    /// <summary>
    ///     Album artist sort order [TS02]
    /// </summary>
    [Description("[TS02]")] SortAlbumArtist,

    /// <summary>
    ///     Artist sort order [TSOP]
    /// </summary>
    [Description("[TSOP]")] SortArtist,

    /// <summary>
    ///     Title sort order [TSOT]
    /// </summary>
    [Description("[TSOT]")] SortTitle,

    /// <summary>
    ///     Content group description [TIT1]
    /// </summary>
    [Description("[TIT1]")] Group,

    /// <summary>
    ///     MovementName [MVNM]
    /// </summary>
    [Description("[MVNM]")] MovementName,

    /// <summary>
    ///     MovementNumber [MVIN]
    /// </summary>
    [Description("[MVIN]")] MovementNumber,

    /// <summary>
    ///     Total parsed number of Movements, no official tag.
    /// </summary>
    [Description("[MovementTotal]")] MovementTotal,

    /// <summary>
    ///     Alternate to MovementNumber and MovementTotal where both are in the same field [MVIN] ('1/5')
    /// </summary>
    [Description("[MVIN]")] MovementNumberTotal,

    /// <summary>
    ///     Long description [TDES]
    /// </summary>
    [Description("[TDES]")] LongDescription,

    /// <summary>
    ///     Beats per minute [TBP, TBPM]
    /// </summary>
    [Description("[TBP, TBPM]")] Bpm,

    /// <summary>
    ///     Person or organization that encoded the file [TEN, TENC]
    /// </summary>
    [Description("[TEN, TENC]")] EncodedBy,


    /*
     *  Original Album date
     *  ID3v2.2 : TOR (year only)
     *  ID3v2.3 : TORY (year only)
     *  ID3v2.4 : TDOR (timestamp according to spec)
     */

    /// <summary>
    ///     Original Album year (when target format only supports year) [TORY,TOR]
    /// </summary>
    [Description("[TOR, TORY, TDOR]")] OrigAlbumYear,

    /// <summary>
    ///     The ‘Date’ frame is a numeric string in the DDMM format containing the date for the recording. This field is always
    ///     four characters long. [TDA, TDAT]
    /// </summary>
    [Description("[TDA, TDAT]")] Date,

    /*
     * Album date
     * ID3v2.2 : no standard
     * ID3v2.3 : no standard
     * ID3v2.4 : TDRL (timestamp according to spec; actual content may vary)
     */

    /// <summary>
    ///     Original date the album was originally released. (when target format supports date), could be a year (TOR, TORY) could be a ISO8601 (TDOR)
    /// </summary>
    [Description("[TDOR]")] OrigAlbumDate,

    /// <summary>
    ///     Album date. [TDRL]
    /// </summary>
    [Description("[TDRL]")] AlbumDate,

    /// <summary>
    ///     Software that encoded the file, with relevant settings if any [TSS, TSSE]
    /// </summary>
    [Description("[TSS, TSSE]")] Encoder,

    /// <summary>
    ///     Language [TLA, TLAN]
    /// </summary>
    [MetaTagMultiValue(true)] [Description("[TLA, TLAN]")]
    Language,

    /// <summary>
    ///     International Standard Recording Code [TRC, TSRC]
    /// </summary>
    [Description("[TRC, TSR]")] Isrc,

    /// <summary>
    ///     Catalog number [CATALOGNUMBER]
    /// </summary>
    [Description("[CATALOGNUMBER]")] CatalogNumber,

    /// <summary>
    ///     Audio source URL [WAS, WOAS]
    /// </summary>
    [Description("[WAS, WOAS]")] AudioSourceUrl,

    /// <summary>
    ///     Lyricist [TXT, TEXT]
    /// </summary>
    [MetaTagMultiValue(true)] [Description("[TXT, TEXT]")]
    Lyricist,
    
    /// <summary>
    /// Mix Engineer 
    /// </summary>
    [Description("TIPL:mix")]
    MixEngineer,
    
    /// <summary>
    /// Mix-DJ
    /// </summary>
    [Description("TIPL:DJ-mix")]
    MixDj,

    /// <summary>
    ///     Mapping between functions (e.g. "producer") and names. Every odd field is a function and every even is a name or a
    ///     comma delimited list of names. [IPL, IPLS, TIPL]
    /// </summary>
    [Description("[IPL, IPLS, TIPL]")] InvolvedPeople,

    /// <summary>
    ///     Represents a user defined URL link ID3 frame. [WXX, WXXX]
    /// </summary>
    [Description("[XX, WXX]")] UserDefinedUrlLink,

    /// <summary>
    ///     [WPAY]
    /// </summary>
    [Description("[WPAY]")] PaymentUrl,

    /// <summary>
    ///     [WCM, WCOM]
    /// </summary>
    [Description("[WCM, WCOM]")] CommercialUrl,

    /// <summary>
    ///     [USER]
    /// </summary>
    [Description("[USER]")] TermsOfUse,

    /// <summary>
    ///     [CNT, PCNT]
    /// </summary>
    [Description("[CNT, PCNT]")] PlayCounter,

    /// <summary>
    ///     [CRA, AENC]
    /// </summary>
    [Description("[CRA, AENC]")] AudioEncryption,

    /// <summary>
    ///     [BUF, RBUF]
    /// </summary>
    [Description("[BUF, RBUF]")] RecommendedBufferSize,

    /// <summary>
    ///     [ETC, ETCO]
    /// </summary>
    [Description("[ETC, ETCO]")] EventTimingCodes,

    /// <summary>
    ///     [EQU, EQU2]
    /// </summary>
    [Description("[EQU, EQU2]")] Equalisation,

    /// <summary>
    ///     [GEO, GEOB]
    /// </summary>
    [Description("[GEO, GEOB]")] GeneralEnscapsulatedObject,

    /// <summary>
    ///     [TLE, TLEN]
    /// </summary>
    [Description("[TLE, TLEN]")] Length,

    /// <summary>
    ///     [LNK, LINK]
    /// </summary>
    [Description("[LNK, LINK]")] LinkedInformation,

    /// <summary>
    ///     [MCI, MCDI]
    /// </summary>
    [Description("[MCI, MCDI]")] MusicCdIdentifier,

    /// <summary>
    ///     [MLL, MLLT]
    /// </summary>
    [Description("[MLL, MLLT]")] MpegLocationLookupTable,

    /// <summary>
    ///     [REV, RVRB]
    /// </summary>
    [Description("[REV, RVRB]")] Reverb,

    /// <summary>
    ///     [RVA, RVA2]
    /// </summary>
    [Description("[RVA, RVA2]")] RelativeVolumeAdjustment,

    /// <summary>
    ///     [STC, SYTC]
    /// </summary>
    [Description("[STC, SYTC]")] SynchronizedTempoCodes,

    /// <summary>
    ///     [TDY, TDLY]
    /// </summary>
    [Description("[TDY, TDLY]")] PlaylistDelay,

    /// <summary>
    ///     [TFT, TFLT]
    /// </summary>
    [Description("[TFT, TFLT]")] FileType,

    /// <summary>
    ///     [TKE, TKEY]
    /// </summary>
    [Description("[TKE, TKEY]")] InitialKey,

    /// <summary>
    ///     [TMT, TMED]
    /// </summary>
    [Description("[TMT, TMED]")] MediaType,

    /// <summary>
    ///     [TOF, TOFN]
    /// </summary>
    [Description("[TOF, TOFN]")] OriginalFilename,

    /// <summary>
    ///     [TOL, TOLY]
    /// </summary>
    [MetaTagMultiValue(true)] [Description("[TOL, TOLY]")]
    OriginalLyricist,
    
    /// <summary>
    /// TIPL:producer
    /// </summary>
    [Description("TIPL:producer")]
    Producer,

    /// <summary>
    ///     [TP4, TPE4]
    /// </summary>
    [Description("[TP4, TPE4]")] InterpretedRemixedOrOtherwiseModifiedBy,

    /// <summary>
    ///     [TXX, TXXX]
    /// </summary>
    [Description("[TXX, TXXX]")] UserDefinedTextInformation,

    /// <summary>
    ///     [UFI, UFID]
    /// </summary>
    [Description("[UFI, UFID]")] UniqueFileIdentifier,

    /// <summary>
    ///     [ULT, USLT]
    /// </summary>
    [Description("[ULT, USLT]")] UnsynchronisedLyrics,

    /// <summary>
    ///     [SYLT]
    /// </summary>
    [Description("[SYLT]")] SynchronisedLyrics,

    /// <summary>
    ///     [WAF, WOAF]
    /// </summary>
    [Description("[WAF, WOAF]")] OfficialAudioFileWebpage,

    /// <summary>
    ///     [WAR, WOAR]
    /// </summary>
    [Description("[WAR, WOAR]")] OfficialArtistPerformerWebpage,

    /// <summary>
    ///     Used when parsing CUE Files.
    /// </summary>
    [Description("[SubCodeFlags]")] SubCodeFlags
}
