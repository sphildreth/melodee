using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.MetaData.Track;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
/// Handle the track title and clean any unwanted text (e.g. Featuring, Year, Deluxe, etc.)
/// </summary>
public sealed partial class TrackTitle(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "79BBF338-6B2F-4166-9F28-97D21C83D2BF";

    public override  string DisplayName => nameof(TrackTitle);

    public override  int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.Title;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags)
    {
        object? tagValue = metaTag.Value;
        var updatedTagValue = false;
        var trackTitle = tagValue as string ?? string.Empty;
        string? featureArtist = null;
        int? trackNumber = null;
        if (trackTitle?.Nullify() != null)
        {
            trackNumber = trackTitle.TryToGetTrackNumberFromString();
            if (trackNumber.HasValue)
            {
                trackTitle = trackTitle.RemoveTrackNumberFromString();
            }
            
            if (trackTitle.HasFeaturingFragments())
            {
                var newTitle = trackTitle ?? string.Empty;
                var matches = StringExtensions.HasFeatureFragmentsRegex.Match(trackTitle!);
                newTitle = newTitle[..matches.Index].CleanString();
                featureArtist = ReplaceTrackArtistSeperators(StringExtensions.HasFeatureFragmentsRegex.Replace(trackTitle!.Substring(matches.Index), string.Empty).CleanString());
                featureArtist = featureArtist?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");
                trackTitle = newTitle;
                updatedTagValue = true;
            }
        }
        var result = new List<MetaTag<object?>>
        {
            new MetaTag<object?>
            {
                Identifier = metaTag.Identifier,
                Value = trackTitle,
                OriginalValue = updatedTagValue ? metaTag.Value : null
            }
        };

        if (trackNumber.HasValue)
        {
            result.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.TrackNumber,
                Value = trackNumber.Value
            });
        }
        
        if (featureArtist?.Nullify() != null)
        {
            result.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.Artist,
                Value = featureArtist
            });
        }
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
    
    private static string? ReplaceTrackArtistSeperators(string? trackArtist)
    {
        return trackArtist.Nullify() == null ? null : ReplaceTrackArtistSeperatorsRegex().Replace(trackArtist!, "/").Trim();
    }

    [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceTrackArtistSeperatorsRegex();
}