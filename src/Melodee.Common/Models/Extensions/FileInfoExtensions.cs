using Melodee.Common.Enums;

namespace Melodee.Common.Models.Extensions;

public static class FileInfoExtensions
{
    public static T? MetaTagValue<T>(this FileInfo fileInfo, MetaTagIdentifier metaTagIdentifier) =>
        (T?)Convert.ChangeType(fileInfo.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value,
            typeof(T));

    public static int TrackNumber(this FileInfo fileInfo) =>
        fileInfo.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;

    public static double TrackDuration(this FileInfo fileInfo) =>
        fileInfo.MetaTagValue<double?>(MetaTagIdentifier.DurationInMs) ?? 0;
    
    public static string? TrackArtist(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.OriginalArtist);    
}