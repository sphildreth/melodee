using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the track Artist and split away any featuring artists.
/// </summary>
public sealed partial class Artist(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "29D61BF9-D283-4DB6-B7EB-16F6BCA76998";

    public override string DisplayName => nameof(Artist);

    public override int SortOrder { get; } = 1;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier is MetaTagIdentifier.Artist;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        var artist = tagValue as string ?? string.Empty;
        string? featureArtist = null;

        var result = new List<MetaTag<object?>>();

        if (artist.Nullify() != null)
        {
            if (Configuration.PluginProcessOptions.ArtistNameReplacements.Any())
            {
                foreach (var kp in Configuration.PluginProcessOptions.ArtistNameReplacements)
                {
                    if (kp.Value.Any(kpv => string.Equals(artist, kpv)))
                    {
                        artist = kp.Key;
                        tagValue = artist;
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = artist,
                            OriginalValue = metaTag.Value
                        });
                        break;
                    }
                }
            }

            if (artist.HasFeaturingFragments())
            {
                var newArtist = artist;
                var matches = StringExtensions.HasFeatureFragmentsRegex.Match(artist);
                newArtist = newArtist[..matches.Index].CleanString();
                featureArtist = ReplaceArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(artist.Substring(matches.Index), string.Empty).CleanString());
                featureArtist = featureArtist?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");
                artist = newArtist;
            }
        }

        if (metaTag.Identifier == MetaTagIdentifier.Artist)
        {
            if (artist.Nullify() != null)
            {
                // If the value for the "Artist" (track artist) matches the "AlbumArtist" (release artist) then nullify the "Artist" value.
                var albumArtistTag = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist);
                if (tagValue?.ToString().DoStringsMatch(albumArtistTag?.Value?.ToString()) ?? false)
                {
                    result.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.Artist,
                        Value = null,
                        OriginalValue = metaTag.Value
                    });
                }

                if (albumArtistTag == null)
                {
                    result.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.AlbumArtist,
                        Value = artist,
                        OriginalValue = metaTag.Value
                    });
                }
            }

            if (featureArtist?.Nullify() != null)
            {
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Artist,
                    Value = featureArtist,
                    OriginalValue = metaTag.Value
                });
            }
        }


        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }

    private static string? ReplaceArtistSeparators(string? trackArtist)
    {
        return trackArtist.Nullify() == null ? null : ReplaceArtistSeparatorsRegex().Replace(trackArtist!, "/").Trim();
    }

    [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceArtistSeparatorsRegex();
}
