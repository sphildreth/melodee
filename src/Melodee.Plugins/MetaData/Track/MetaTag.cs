using System.Reflection;
using Melodee.Common.Enums;
using Melodee.Common.Exceptions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Utility;
using Melodee.Plugins.Discovery;
using Melodee.Plugins.MetaData.Track.Extensions;

namespace Melodee.Plugins.MetaData.Track;

public sealed class MetaTag : MetaDataBase, ITrackPlugin
{
    public override string Id => "0F622E4B-64CD-4033-8B23-BA2001F045FA";
    
    public override string DisplayName => nameof(MetaTag);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists)
        {
            return false;
        }
        return FileHelper.IsFileMediaType(fileSystemInfo.Extension);
    }

    public Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        var tags = new List<MetaTag<object>>();        
        if (fileSystemInfo.Exists)
        {
            var fileAtl = new ATL.Track(fileSystemInfo.FullName);
            if (!fileAtl.MetadataFormats.Any(x => x.ID < 0) && IsAtlTrackForMp3(fileAtl))
            {
                var atlDictionary = fileAtl.ToDictionary();
                var metaTagIdentifierDictionary = MetaTagIdentifier.NotSet.ToDictionary();

                foreach (var metaTagIdentifier in metaTagIdentifierDictionary)
                {
                    if (atlDictionary.TryGetValue(metaTagIdentifier.Value, out var v))
                    {
                        if (v is string s)
                        {
                            v = s.Nullify();
                        }
                        if (v is DateTime vDt)
                        {
                            v = vDt > DateTime.MinValue && vDt < DateTime.MaxValue ? v : null;
                        }
                        if (v != null)
                        {
                            var identifier = SafeParser.ToEnum<MetaTagIdentifier>(metaTagIdentifier.Key);
                            if (metaTagIdentifier.Key == (int)MetaTagIdentifier.Date ||
                                metaTagIdentifier.Key == (int)MetaTagIdentifier.RecordingDate)
                            {
                                var dt = SafeParser.ToDateTime(v);
                                if (dt.HasValue)
                                {
                                    tags.Add(new MetaTag<object>
                                    {
                                        Identifier = MetaTagIdentifier.OrigReleaseYear,
                                        Value = dt.Value.Year
                                    });
                                }
                            }
                            tags.Add(new MetaTag<object>
                            {
                                Identifier = identifier,
                                Value = v
                            });
                        }
                    }
                }
            }
        }
        return Task.FromResult(new OperationResult<Common.Models.Track>
        {
            Data = new Common.Models.Track
            {
                FileSystemInfo = fileSystemInfo,
                Tags = tags
            }
        });
    }
    
    private static bool IsAtlTrackForMp3(ATL.Track track)
    {
        if(track?.AudioFormat?.ShortName == null)
        {
            return false;
        }
        if(string.Equals(track.AudioFormat?.ShortName, "mpeg-4", StringComparison.OrdinalIgnoreCase))
        {
            var ext = track.FileInfo().Extension;
            if(!ext.ToLower().EndsWith("m4a")) // M4A is an audio file using the MP4 encoding
            {
                Console.WriteLine($"Video file found in Scanning. File [{ track.FileInfo().FullName }]");
                return false;
            }
        }
        return track is { AudioFormat: { ID: > -1 }, Duration: > 0 };
    }    
    
    // https://exiftool.org/TagNames/ID3.html
    
    // // Fields allowed to have multiple values according to ID3v2.2-3 specs
    // private static readonly ISet<string> multipleValuev23Fields = new HashSet<string> { "TP1", "TCM", "TXT", "TLA", "TOA", "TOL", "TCOM", "TEXT", "TOLY", "TOPE", "TPE1" };
    //
    // // Note on date field identifiers
    // //
    // // Original release date
    // //   ID3v2.2 : TOR (year only)
    // //   ID3v2.3 : TORY (year only)
    // //   ID3v2.4 : TDOR (timestamp according to spec)
    // //
    // // Release date
    // //   ID3v2.2 : no standard
    // //   ID3v2.3 : no standard
    // //   ID3v2.4 : TDRL (timestamp according to spec; actual content may vary)
    // //
    // // Recording date <== de facto standard behind the "date" field on most taggers
    // //   ID3v2.2 : TYE (year), TDA (day & month - DDMM), TIM (hour & minute - HHMM)
    // //   ID3v2.3 : TYER (year), TDAT (day & month - DDMM), TIME (hour & minute - HHMM)
    // //   NB : Some loose implementations actually use TDRC inside ID3v2.3 headers (MediaMonkey, I'm looking at you...)
    // //   ID3v2.4 : TDRC (timestamp)    

        // // Mapping between standard fields and ID3v2.2 identifiers
        // private static readonly IDictionary<string, Field> frameMapping_v22 = new Dictionary<string, Field>
        //     {
        //         { "TT1", Field.GENERAL_DESCRIPTION },
        //         { "TT2", Field.TITLE },
        //         { "TP1", Field.ARTIST },
        //         { "TP2", Field.ALBUM_ARTIST },  // De facto standard, regardless of spec
        //         { "TP3", Field.CONDUCTOR },
        //         { "TOA", Field.ORIGINAL_ARTIST },
        //         { "TAL", Field.ALBUM },
        //         { "TOT", Field.ORIGINAL_ALBUM },
        //         { "TRK", Field.TRACK_NUMBER_TOTAL },
        //         { "TPA", Field.DISC_NUMBER_TOTAL },
        //         { "TYE", Field.RECORDING_YEAR },
        //         { "TDA", Field.RECORDING_DAYMONTH },
        //         { "TIM", Field.RECORDING_TIME },
        //         { "COM", Field.COMMENT },
        //         { "TCM", Field.COMPOSER },
        //         { "POP", Field.RATING },
        //         { "TCO", Field.GENRE },
        //         { "TCR", Field.COPYRIGHT },
        //         { "TPB", Field.PUBLISHER },
        //         { "TBP", Field.BPM },
        //         { "TEN", Field.ENCODED_BY },
        //         { "TOR", Field.ORIG_RELEASE_YEAR },
        //         { "TSS", Field.ENCODER },
        //         { "TLA", Field.LANGUAGE },
        //         { "TRC", Field.ISRC },
        //         { "WAS", Field.AUDIO_SOURCE_URL },
        //         { "TXT", Field.LYRICIST },
        //         { "IPL", Field.INVOLVED_PEOPLE }
        //     };
        //
        // // Mapping between standard fields and ID3v2.3 identifiers
        // private static readonly IDictionary<string, Field> frameMapping_v23 = new Dictionary<string, Field>
        //     {
        //         { "TIT2", Field.TITLE },
        //         { "TPE1", Field.ARTIST },
        //         { "TPE2", Field.ALBUM_ARTIST }, // De facto standard, regardless of spec
        //         { "TPE3", Field.CONDUCTOR },
        //         { "TOPE", Field.ORIGINAL_ARTIST },
        //         { "TALB", Field.ALBUM },
        //         { "TOAL", Field.ORIGINAL_ALBUM },
        //         { "TRCK", Field.TRACK_NUMBER_TOTAL },
        //         { "TPOS", Field.DISC_NUMBER_TOTAL },
        //         { "TYER", Field.RECORDING_YEAR },
        //         { "TDAT", Field.RECORDING_DAYMONTH },
        //         { "TDRC", Field.RECORDING_DATE }, // Not part of ID3v2.3 standard, but sometimes found there anyway (MediaMonkey, I'm looking at you...)
        //         { "TIME", Field.RECORDING_TIME },
        //         { "COMM", Field.COMMENT },
        //         { "TCOM", Field.COMPOSER },
        //         { "POPM", Field.RATING },
        //         { "TCON", Field.GENRE },
        //         { "TCOP", Field.COPYRIGHT },
        //         { "TPUB", Field.PUBLISHER },
        //         { "CTOC", Field.CHAPTERS_TOC_DESCRIPTION },
        //         { "TSOA", Field.SORT_ALBUM },
        //         { "TSO2", Field.SORT_ALBUM_ARTIST }, // Not part of ID3v2.3 standard
        //         { "TSOP", Field.SORT_ARTIST },
        //         { "TSOT", Field.SORT_TITLE },
        //         { "TIT1", Field.GROUP },
        //         { "MVIN", Field.SERIES_PART}, // Not part of ID3v2.3 standard
        //         { "MVNM", Field.SERIES_TITLE }, // Not part of ID3v2.3 standard
        //         { "TDES", Field.LONG_DESCRIPTION }, // Not part of ID3v2.3 standard
        //         { "TBPM", Field.BPM },
        //         { "TENC", Field.ENCODED_BY },
        //         { "TORY", Field.ORIG_RELEASE_YEAR},
        //         { "TSSE", Field.ENCODER },
        //         { "TLAN", Field.LANGUAGE },
        //         { "TSRC", Field.ISRC },
        //         { "CATALOGNUMBER", Field.CATALOG_NUMBER },
        //         { "WOAS", Field.AUDIO_SOURCE_URL },
        //         { "TEXT", Field.LYRICIST },
        //         { "IPLS", Field.INVOLVED_PEOPLE }
        //     };
        //
        // // Mapping between standard fields and ID3v2.4 identifiers
        // private static readonly IDictionary<string, Field> frameMapping_v24 = new Dictionary<string, Field>
        //     {
        //         { "TIT2", Field.TITLE },
        //         { "TPE1", Field.ARTIST },
        //         { "TPE2", Field.ALBUM_ARTIST }, // De facto standard, regardless of spec
        //         { "TPE3", Field.CONDUCTOR },
        //         { "TOPE", Field.ORIGINAL_ARTIST },
        //         { "TALB", Field.ALBUM },
        //         { "TOAL", Field.ORIGINAL_ALBUM },
        //         { "TRCK", Field.TRACK_NUMBER_TOTAL },
        //         { "TPOS", Field.DISC_NUMBER_TOTAL },
        //         { "TDRC", Field.RECORDING_DATE },
        //         { "COMM", Field.COMMENT },
        //         { "TCOM", Field.COMPOSER },
        //         { "POPM", Field.RATING },
        //         { "TCON", Field.GENRE },
        //         { "TCOP", Field.COPYRIGHT },
        //         { "TPUB", Field.PUBLISHER },
        //         { "CTOC", Field.CHAPTERS_TOC_DESCRIPTION },
        //         { "TDRL", Field.PUBLISHING_DATE },
        //         { "TSOA", Field.SORT_ALBUM },
        //         { "TSO2", Field.SORT_ALBUM_ARTIST }, // Not part of ID3v2.4 standard
        //         { "TSOP", Field.SORT_ARTIST },
        //         { "TSOT", Field.SORT_TITLE },
        //         { "TIT1", Field.GROUP },
        //         { "MVIN", Field.SERIES_PART}, // Not part of ID3v2.4 standard
        //         { "MVNM", Field.SERIES_TITLE }, // Not part of ID3v2.4 standard
        //         { "TDES", Field.LONG_DESCRIPTION }, // Not part of ID3v2.4 standard
        //         { "TBPM", Field.BPM },
        //         { "TENC", Field.ENCODED_BY },
        //         { "TDOR", Field.ORIG_RELEASE_DATE },
        //         { "TSSE", Field.ENCODER },
        //         { "TLAN", Field.LANGUAGE},
        //         { "TSRC", Field.ISRC},
        //         { "CATALOGNUMBER", Field.CATALOG_NUMBER },
        //         { "WOAS", Field.AUDIO_SOURCE_URL},
        //         { "TEXT", Field.LYRICIST },
        //         { "TIPL", Field.INVOLVED_PEOPLE }
        //     };
        //
        // // Mapping between ID3v2.2/3 fields and ID3v2.4 fields not included in frameMapping_v2x, and that have changed between versions
        // private static readonly IDictionary<string, string> frameMapping_v22_4 = new Dictionary<string, string>
        //     {
        //         { "BUF", "RBUF" },
        //         { "CNT", "PCNT" },
        //         { "CRA", "AENC" },
        //         // CRM / Encrypted meta frame field has been droppped
        //         { "ETC", "ETCO" },
        //         { "EQU", "EQU2" },
        //         { "GEO", "GEOB" },
        //         { "IPL", "TIPL" },
        //         { "LNK", "LINK" },
        //         { "MCI", "MCDI" },
        //         { "MLL", "MLLT" },
        //         { "REV", "RVRB" },
        //         { "RVA", "RVA2" },
        //         { "SLT", "SYLT" },
        //         { "STC", "SYTC" },
        //         { "TBP", "TBPM" },
        //         { "TDY", "TDLY" },
        //         { "TEN", "TENC" },
        //         { "TFT", "TFLT" },
        //         { "TKE", "TKEY" },
        //         { "TLA", "TLAN" },
        //         { "TLE", "TLEN" },
        //         { "TMT", "TMED" },
        //         { "TOF", "TOFN" },
        //         { "TOL", "TOLY" },
        //         { "TP4", "TPE4" },
        //         { "TPA", "TPOS" },
        //         { "TRC", "TSRC" },
        //         //{ "TRD", "" } no direct equivalent
        //         // TSI / Size field has been dropped
        //         { "TSS", "TSSE" },
        //         { "TT3", "TIT3" },
        //         { "TXT", "TEXT" },
        //         { "TXX", "TXXX" },
        //         { "UFI", "UFID" },
        //         { "ULT", "USLT" },
        //         { "WAF", "WOAF" },
        //         { "WAR", "WOAR" },
        //         { "WAS", "WOAS" },
        //         { "WCM", "WCOM" },
        //         { "WCP", "WCOP" },
        //         { "WPB", "WPUB" },
        //         { "WXX", "WXXX" }
        //         // TYE, TDA and TIM are converted on the fly when writing
        // };
        // private static readonly IDictionary<string, string> frameMapping_v23_4 = new Dictionary<string, string>
        //     {
        //         { "EQUA", "EQU2" },
        //         { "IPLS", "TIPL" },
        //         { "RVAD", "RVA2" },
        //         { "TORY", "TDOR" } // yyyy is a valid timestamp
        //         // TYER, TDAT and TIME are converted on the fly when writing
        //     };    
    
}