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

    public override int SortOrder { get; } = 2; // Should run after Artist

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

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value as string;
        var updatedTagValue = false;
        var trackTitle = tagValue ?? string.Empty;
        var result = new List<MetaTag<object?>>();
        
        int? trackNumber = null;
        var metaTagsValue = metaTags?.ToArray() ?? [];
        if (trackTitle?.Nullify() != null)
        {
            if (ContinueProcessing(trackTitle))
            {
                trackNumber = metaTagsValue.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? trackTitle.TryToGetTrackNumberFromString();
                if ((trackNumber ?? 0) > 0)
                {
                    trackTitle = trackTitle.RemoveTrackNumberFromString();
                }
            }

            if (ContinueProcessing(trackTitle))
            {
                if (trackTitle.HasFeaturingFragments() || trackTitle.HasWithFragments())
                {
                    string? featureArtist = null;
                    string? newTrackTitle = null;
                    
                    if (trackTitle.HasFeaturingFragments())
                    {
                        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(trackTitle!);
                        featureArtist = MetaTagsProcessor.ReplaceTrackArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(trackTitle!.Substring(matches.Index), string.Empty).CleanString());
                        newTrackTitle = TrackTitleWithoutFeaturingArtist(trackTitle);                        
                    }

                    if (trackTitle.HasWithFragments())
                    {
                        var matches = StringExtensions.HasWithFragmentsRegex.Match(trackTitle!);
                        featureArtist = MetaTagsProcessor.ReplaceTrackArtistSeparators(StringExtensions.HasWithFragmentsRegex.Replace(trackTitle!.Substring(matches.Index), string.Empty).CleanString());
                        newTrackTitle = TrackTitleWithoutWithArtist(trackTitle);                          
                    }

                    featureArtist = featureArtist?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");

                    if (featureArtist.Nullify() != null)
                    {
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = featureArtist
                        });                        
                    }
                    
                    if (!trackTitle.DoStringsMatch(newTrackTitle))
                    {
                        trackTitle = newTrackTitle;
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
            
            if (trackTitle.HasFeaturingFragments())
            {
                trackTitle = TrackArtistFromReleaseArtistViaFeaturing(trackTitle);
            }            
        }
       
        if (trackTitle.Nullify() != null)
        {
            result.Add(
                new()
                {
                    Identifier = metaTag.Identifier,
                    Value = trackTitle,
                    OriginalValue = updatedTagValue ? metaTag.Value : null
                }
            );
        }

        if (trackNumber.HasValue)
        {
            result.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.TrackNumber,
                Value = trackNumber.Value
            });
        }
        result.ForEach(x => x.AddProcessedBy(nameof(Artist)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
