using Melodee.Common.Enums;

namespace Melodee.Common.Models.Extensions;

public static class ReleaseExtensions
{
    public static T? MetaTagValue<T>(this Release release, MetaTagIdentifier metaTagIdentifier) =>
        (T?)Convert.ChangeType(release.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value,
            typeof(T));

    public static string? Artist(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Artist);

    public static int MediaCountValue(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
    
    public static int TrackCountValue(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.TrackTotal) ?? 0;
}