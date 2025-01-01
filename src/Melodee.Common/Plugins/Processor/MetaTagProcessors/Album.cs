using System.Text.RegularExpressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Common.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the Album title (Album) and clean any unwanted text (e.g. Featuring, Year, Deluxe, etc.)
/// </summary>
public sealed class Album(Dictionary<string, object?> configuration, ISerializer serializer) : MetaTagProcessorBase(configuration, serializer)
{
    private static readonly Regex YearInAlbumTitleRegex = new(@"(\[|\()+[0-9]+(\]|\))+", RegexOptions.Compiled);

    public static readonly Regex UnwantedAlbumTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|Album|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|disk|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex UnwantedRemasterWithYearRegex = new(@"\s[\[\(]*(re(\-*)issue(ed*)|re(\-*)master(ed*)(\s*[0-9]*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override string Id => "B7288065-E229-41A6-9A2C-E14F5F6B0FE7";

    public override string DisplayName => nameof(Album);

    public override int SortOrder { get; } = 10;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.Album;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        if (tagValue == null)
        {
            return new OperationResult<IEnumerable<MetaTag<object?>>>
            {
                Data = new[]
                {
                    new MetaTag<object?>
                    {
                        Identifier = metaTag.Identifier,
                        Value = null
                    }
                }
            };
        }

        var albumTitle = tagValue as string ?? string.Empty;

        var newAlbumTitle = albumTitle;
        try
        {
            var matches1 = UnwantedRemasterWithYearRegex.Match(albumTitle);
            if (matches1.Success)
            {
                newAlbumTitle = newAlbumTitle[..matches1.Index].CleanString();
            }

            var matches = UnwantedAlbumTitleTextRegex.Match(albumTitle);
            if (matches.Length > 0)
            {
                newAlbumTitle = newAlbumTitle?[..matches.Index].CleanString();
                var lastIndexOfOpenParenthesis = newAlbumTitle?.LastIndexOf('(') ?? 0;
                var lastIndexOfCloseParenthesis = newAlbumTitle?.LastIndexOf(')') ?? 0;
                if (newAlbumTitle != null && lastIndexOfOpenParenthesis > 0 && lastIndexOfCloseParenthesis < 0)
                {
                    newAlbumTitle = newAlbumTitle.Substring(0, lastIndexOfOpenParenthesis - 1);
                }

                var lastIndexOfOpenBracket = newAlbumTitle?.LastIndexOf('[') ?? 0;
                var lastIndexOfCloseBracket = newAlbumTitle?.LastIndexOf(']') ?? 0;
                if (newAlbumTitle != null && lastIndexOfOpenBracket > 0 && lastIndexOfCloseBracket < 0)
                {
                    newAlbumTitle = newAlbumTitle.Substring(0, lastIndexOfOpenBracket - 1);
                }
            }

            var yearInTextMatches = YearInAlbumTitleRegex.Match(albumTitle);
            if (yearInTextMatches.Length > 0)
            {
                newAlbumTitle = YearInAlbumTitleRegex.Replace(newAlbumTitle ?? string.Empty, string.Empty).CleanString();
            }

            var albumArtistTag = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist);
            if (albumArtistTag?.Value != null)
            {
                // If the artist name is in the album title, but not a self-titled album, then remove the artist name from the title.                
                var artistValue = albumArtistTag.Value as string ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(artistValue) &&
                    !string.Equals(newAlbumTitle, artistValue, StringComparison.OrdinalIgnoreCase) &&
                    (newAlbumTitle?.Contains(artistValue, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    newAlbumTitle = newAlbumTitle.Replace(artistValue, string.Empty).ToAlphanumericName(false, false).ToTitleCase()?.CleanString();
                }
            }
            else
            {
                throw new Exception($"Unable to find Album Artist For [{DisplayName}]");
            }

            var artistTag = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist);
            if (artistTag?.Value != null)
            {
                var artistValue = artistTag.Value as string ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(artistValue) && (newAlbumTitle?.Contains(artistValue, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    newAlbumTitle = newAlbumTitle.Replace(artistValue, string.Empty).ToAlphanumericName(false, false).ToTitleCase()?.CleanString();
                }
            }

            if (newAlbumTitle != null)
            {
                var albumTitleReplacements = MelodeeConfiguration.FromSerializedJsonArray(Configuration[SettingRegistry.ProcessingAlbumTitleRemovals], Serializer);
                if (albumTitleReplacements.Any())
                {
                    newAlbumTitle = albumTitleReplacements.Aggregate(newAlbumTitle, (current, replacement) => current.Replace(replacement, string.Empty, StringComparison.OrdinalIgnoreCase)).Trim();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{PluginName}] attempting to process [{MetaTag}]", DisplayName, metaTag);
        }

        var wasTagValueModified = !string.Equals(newAlbumTitle, albumTitle, StringComparison.OrdinalIgnoreCase);
        var result = new List<MetaTag<object?>>
        {
            new()
            {
                Identifier = metaTag.Identifier,
                Value = newAlbumTitle,
                OriginalValue = wasTagValueModified ? metaTag.Value : null
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(Album)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
