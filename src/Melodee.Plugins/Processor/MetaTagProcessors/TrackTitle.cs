using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Utility;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the track title and clean any unwanted text (e.g. Featuring, Year, Deluxe, etc.)
/// </summary>
public sealed class TrackTitle(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "79BBF338-6B2F-4166-9F28-97D21C83D2BF";

    public override string DisplayName => nameof(TrackTitle);

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.Title;
    }

    private bool ContinueProcessing(string? trackTitle)
    {
        // If Track Title is just a number (Knife Party - Abondon Ship - 404) then don't modify.
        if (SafeParser.ToNumber<int>(trackTitle) > 0)
        {
            return false;
        }

        return true;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value as string;
        var updatedTagValue = false;
        var trackTitle = tagValue ?? string.Empty;
        string? featureArtist = null;
        int? trackNumber = null;
        if (trackTitle?.Nullify() != null)
        {
            if (ContinueProcessing(trackTitle))
            {
                trackNumber = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? trackTitle.TryToGetTrackNumberFromString();
                if ((trackNumber ?? 0) > 0)
                {
                    trackTitle = trackTitle.RemoveTrackNumberFromString();
                }
            }

            if (ContinueProcessing(trackTitle))
            {
                if (trackTitle.HasFeaturingFragments())
                {
                    var newTitle = trackTitle ?? string.Empty;
                    var matches = StringExtensions.HasFeatureFragmentsRegex.Match(trackTitle!);
                    if (matches.Index > 0)
                    {
                        newTitle = newTitle[..matches.Index].CleanString();
                        featureArtist = MetaTagsProcessor.ReplaceTrackArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(trackTitle!.Substring(matches.Index), string.Empty).CleanString());
                        featureArtist = featureArtist?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");
                        trackTitle = newTitle;
                        updatedTagValue = true;
                    }
                }
            }

            if (ContinueProcessing(trackTitle))
            {
                if (trackTitle != null && Configuration.PluginProcessOptions.TrackTitleRemovals.Any())
                {
                    trackTitle = Configuration.PluginProcessOptions.TrackTitleRemovals.Aggregate(trackTitle, (current, replacement) => current.Replace(replacement, string.Empty, StringComparison.OrdinalIgnoreCase)).Trim();
                    updatedTagValue = trackTitle != tagValue;
                }
            }
        }

        var result = new List<MetaTag<object?>>
        {
            new()
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
}
