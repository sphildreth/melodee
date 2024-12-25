using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the Song title and clean any unwanted text (e.g. Featuring, Year, Deluxe, etc.)
/// </summary>
public sealed class SongTitle(Dictionary<string, object?> configuration, ISerializer serializer) : MetaTagProcessorBase(configuration, serializer)
{
    public override string Id => "79BBF338-6B2F-4166-9F28-97D21C83D2BF";

    public override string DisplayName => nameof(SongTitle);

    public override int SortOrder { get; } = 2; // Should run after Artist

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.Title;
    }

    private bool ContinueProcessing(string? songTitle)
    {
        // If Song Title is just a number (Knife Party - Abondon Ship - 404) then don't modify.
        if (SafeParser.ToNumber<int>(songTitle) > 0)
        {
            return false;
        }

        return true;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value as string;
        var updatedTagValue = false;
        var songTitle = tagValue ?? string.Empty;
        var result = new List<MetaTag<object?>>();

        int? songNumber = null;
        var metaTagsValue = metaTags?.ToArray() ?? [];
        if (songTitle?.Nullify() != null)
        {
            var matches1 = Album.UnwantedRemasterWithYearRegex.Match(songTitle);
            if (matches1.Success)
            {
                songTitle = songTitle[..matches1.Index].CleanString();
            }
            if (ContinueProcessing(songTitle))
            {
                songNumber = metaTagsValue.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? songTitle?.TryToGetSongNumberFromString();
                if ((songNumber ?? 0) > 0)
                {
                    songTitle = songTitle?.RemoveSongNumberFromString();
                }
            }

            if (ContinueProcessing(songTitle))
            {
                if (songTitle.HasFeaturingFragments() || songTitle.HasWithFragments())
                {
                    string? featureArtist = null;
                    string? newSongTitle = null;

                    if (songTitle.HasFeaturingFragments())
                    {
                        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(songTitle!);
                        featureArtist = MetaTagsProcessor.ReplaceSongArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(songTitle!.Substring(matches.Index), string.Empty).CleanString());
                        newSongTitle = SongTitleWithoutFeaturingArtist(songTitle);
                    }

                    if (songTitle.HasWithFragments())
                    {
                        var matches = StringExtensions.HasWithFragmentsRegex.Match(songTitle!);
                        featureArtist = MetaTagsProcessor.ReplaceSongArtistSeparators(StringExtensions.HasWithFragmentsRegex.Replace(songTitle!.Substring(matches.Index), string.Empty).CleanString());
                        newSongTitle = SongTitleWithoutWithArtist(songTitle);
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

                    if (!songTitle.DoStringsMatch(newSongTitle))
                    {
                        songTitle = newSongTitle;
                        updatedTagValue = true;
                    }
                }
            }

            if (ContinueProcessing(songTitle))
            {
                var songTitleRemovals = MelodeeConfiguration.FromSerializedJsonArray(Configuration[SettingRegistry.ProcessingSongTitleRemovals], Serializer);
                if (songTitle != null && songTitleRemovals.Any())
                {
                    songTitle = songTitleRemovals.Aggregate(songTitle, (current, replacement) => current.Replace(replacement, string.Empty, StringComparison.OrdinalIgnoreCase)).Trim();
                    updatedTagValue = songTitle != tagValue;
                }
            }

            if (songTitle.HasFeaturingFragments())
            {
                songTitle = SongArtistFromAlbumArtistViaFeaturing(songTitle);
            }
        }

        if (songTitle.Nullify() != null)
        {
            result.Add(
                new MetaTag<object?>
                {
                    Identifier = metaTag.Identifier,
                    Value = songTitle,
                    OriginalValue = updatedTagValue ? metaTag.Value : null
                }
            );
        }

        if (songNumber.HasValue)
        {
            result.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.TrackNumber,
                Value = songNumber.Value
            });
        }

        result.ForEach(x => x.AddProcessedBy(nameof(SongTitle)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
