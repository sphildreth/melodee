using Melodee.Common.Enums;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class TrackExtensions
{
    public static T? MetaTagValue<T>(this Track track, MetaTagIdentifier metaTagIdentifier) =>
        (T?)Convert.ChangeType(track.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value,
            typeof(T));
    
    public static double? Duration(this Track track) => track.MetaTagValue<double?>(MetaTagIdentifier.DurationInMs);  

    public static string? DurationTime (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration().Value)).ToFullFormattedString() : "--:--";

    public static string? DurationTimeShort (this Track track) => track.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(track.Duration().Value)).ToShortFormattedString() : "--:--";
    
    public static string? Title(this Track track) => track.MetaTagValue<string>(MetaTagIdentifier.Title);

    public static int TrackNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;

    public static int MediaNumber(this Track track) => track.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
}