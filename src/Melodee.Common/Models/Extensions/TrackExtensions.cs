using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Common.Models.Extensions;

public static class TrackExtensions
{
    private static readonly Regex UnwantedTrackTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static T? MetaTagValue<T>(this Track track, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (track.Tags == null || !track.Tags.Any())
        {
            return d;
        }
        try
        {
            var vv = track.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }
            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }            
           var converter = TypeDescriptor.GetConverter(typeof(T?));
           return (T?)converter.ConvertFrom(vv);           
        }
        catch (Exception e)
        {
            Log.Debug(e, "Track [{Track}]", track);
        }
        return d;
    }
    
    public static T? MediaAudioValue<T>(this Track track, MediaAudioIdentifier mediaAudioIdentifier)
    {
        var d = default(T?);
        if (track.MediaAudios == null || !track.MediaAudios.Any())
        {
            return d;
        }
        try
        {
            var vv = track.MediaAudios?.FirstOrDefault(x => x.Identifier == mediaAudioIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T?));
            return (T?)converter.ConvertFrom(vv);           
        }
        catch (Exception e)
        {
            Log.Debug(e, "Track [{Track}]", track);
        }
        return d;
    }    

    public static string? TrackArtist(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Artist);

    public static string? ReleaseArtist(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist);
    
    public static string? ReleaseTitle(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Album);
    
    public static int? ReleaseYear(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ?? 
                                                        track.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                        track.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);   
    
    public static int? TrackYear(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ?? 
                                                          track.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                          track.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);    
    
    public static double? Duration(this Track track) => track.MediaAudioValue<double?>(MediaAudioIdentifier.DurationMs);  

    public static string? DurationTime (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration()!.Value)).ToFullFormattedString() : "--:--";

    public static string? DurationTimeShort (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration()!.Value)).ToShortFormattedString() : "--:--";
    
    public static string? Title(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Title);

    public static int TrackNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;
    
    public static int TrackTotalNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.TrackTotal) ?? 0;

    public static int MediaNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
    
    public static int MediaTotalNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal) ??
                                                            track.MetaTagValue<int?>(MetaTagIdentifier.DiscNumberTotal) ?? 0;

    public static bool TitleHasUnwantedText(this Track track)
    {
        var releaseTitle = track.ReleaseTitle() ?? string.Empty;
        var trackTitle = track.Title();
        if (string.IsNullOrWhiteSpace(trackTitle))
        {
            return true;
        }
        if (trackTitle.HasFeaturingFragments())
        {
            return true;
        }
        try
        {
            if (UnwantedTrackTitleTextRegex.IsMatch(trackTitle))
            {
                return true;
            }
            if (trackTitle.Any(char.IsDigit))
            {
                var trackNumber = track.TrackNumber();
                if (string.Equals(trackTitle.Trim(), trackNumber.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return Regex.IsMatch(trackTitle, $@"^({Regex.Escape(releaseTitle)}\s*.*\s*)?([0-9]*{trackNumber}\s)");
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"TitleHasUnwantedText For ReleaseTitle [{releaseTitle}] for TrackTitle [{trackTitle}] Error [{ex.Message}] ", "Error");
        }
        return false;        
    }

    public static string TrackFileName(int trackNumber, string? trackTitle, int mediaNumber)
    {
        if (trackNumber < 1)
        {
            throw new Exception($"Invalid track number [{ trackNumber }]");
        }
        if (string.IsNullOrWhiteSpace(trackTitle))
        {
            throw new Exception($"Invalid track title [{ trackTitle }]");
        }
        
        var trackNumberValue = trackNumber.ToString("D3");
        
        // Put an "m" for media on the TPOS greater than 1 so the directory sorts proper
        /*
          Example when not part of a media set track 7
          "007 Something.mpg"
                    
          Example media 1 track 14
          "001-014 Something.mpg"
          
          Example media 2 track 5
          "002-005 Something Else.mpg"
        */
        var disc = mediaNumber > 1 ? $"{mediaNumber:D3}-" : string.Empty;

        // Get new name for file
        var fileNameFromTitle = trackTitle.ToTitleCase(false)?.ToFileNameFriendly();
        if (fileNameFromTitle != null && fileNameFromTitle.StartsWith(trackNumberValue))
        {
            fileNameFromTitle = fileNameFromTitle
                .RemoveStartsWith($"{trackNumberValue} -")
                .RemoveStartsWith($"{trackNumberValue} ")
                .RemoveStartsWith($"{trackNumberValue}.")
                .RemoveStartsWith($"{trackNumberValue}-")
                .ToTitleCase(false);
        }
        return $"{disc}{trackNumberValue} {fileNameFromTitle}.mp3";
    }

    public static string ToTrackFileName(this Track track)
        => TrackFileName(track.TrackNumber(), track.Title(), track.MediaNumber());
}