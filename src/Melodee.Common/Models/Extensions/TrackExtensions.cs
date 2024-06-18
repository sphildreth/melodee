using System.ComponentModel;
using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class TrackExtensions
{
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
           var converter = TypeDescriptor.GetConverter(typeof(T?));
           return (T?)converter.ConvertFrom(vv);           
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }
        return d;
    }

    public static string? TrackArtist(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.OriginalArtist);

    public static string? Artist(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist) ??
                                                        track.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    
    public static string? ReleaseTitle(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Title);
    
    public static int? ReleaseYear(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ?? 
                                                        track.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                        track.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);    
    
    public static string? TrackTitle(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Album);
    
    public static int? TrackYear(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseDate) ?? 
                                                          track.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                          track.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);    
    
    public static double? Duration(this Track track) => track.MetaTagValue<double?>(MetaTagIdentifier.DurationInMs);  

    public static string? DurationTime (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration().Value)).ToFullFormattedString() : "--:--";

    public static string? DurationTimeShort (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration().Value)).ToShortFormattedString() : "--:--";
    
    public static string? Title(this Track track) => track.MetaTagValue<string?>(MetaTagIdentifier.Title);

    public static int TrackNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;

    public static int MediaNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
}