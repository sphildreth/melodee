using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Serilog;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the release title (Album) and clean any unwanted text (e.g. Featuring, Year, Deluxe, etc.)
/// </summary>
public sealed class Album(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    private static readonly Regex YearInReleaseTitleRegex = new(@"(\[|\()+[0-9]+(\]|\))+", RegexOptions.Compiled);

    private static readonly Regex UnwantedReleaseTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|release|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|disk|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

        var releaseTitle = tagValue as string ?? string.Empty;

        var newReleaseTitle = releaseTitle;
        try
        {
            var matches = UnwantedReleaseTitleTextRegex.Match(releaseTitle);
            if (matches.Length > 0)
            {
                newReleaseTitle = newReleaseTitle[..matches.Index].CleanString();
                var lastIndexOfOpenParenthesis = newReleaseTitle?.LastIndexOf('(') ?? 0;
                var lastIndexOfCloseParenthesis = newReleaseTitle?.LastIndexOf(')') ?? 0;
                if (newReleaseTitle != null && lastIndexOfOpenParenthesis > 0 && lastIndexOfCloseParenthesis < 0)
                {
                    newReleaseTitle = newReleaseTitle.Substring(0, lastIndexOfOpenParenthesis - 1);
                }

                var lastIndexOfOpenBracket = newReleaseTitle?.LastIndexOf('[') ?? 0;
                var lastIndexOfCloseBracket = newReleaseTitle?.LastIndexOf(']') ?? 0;
                if (newReleaseTitle != null && lastIndexOfOpenBracket > 0 && lastIndexOfCloseBracket < 0)
                {
                    newReleaseTitle = newReleaseTitle.Substring(0, lastIndexOfOpenBracket - 1);
                }
            }

            var yearInTextMatches = YearInReleaseTitleRegex.Match(releaseTitle);
            if (yearInTextMatches.Length > 0)
            {
                newReleaseTitle = YearInReleaseTitleRegex.Replace(newReleaseTitle ?? string.Empty, string.Empty).CleanString();
            }

            var albumArtistTag = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist);
            if (albumArtistTag?.Value != null)
            {
                var artistValue = albumArtistTag.Value as string ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(artistValue) && (newReleaseTitle?.Contains(artistValue, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    newReleaseTitle = newReleaseTitle.Replace(artistValue, string.Empty).ToAlphanumericName(false, false)?.ToTitleCase()?.CleanString();
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
                if (!string.IsNullOrWhiteSpace(artistValue) && (newReleaseTitle?.Contains(artistValue, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    newReleaseTitle = newReleaseTitle.Replace(artistValue, string.Empty).ToAlphanumericName(false, false)?.ToTitleCase()?.CleanString();
                }
            }

            if (newReleaseTitle != null && Configuration.PluginProcessOptions.ReleaseTitleRemovals.Any())
            {
                newReleaseTitle = Configuration.PluginProcessOptions.ReleaseTitleRemovals.Aggregate(newReleaseTitle, (current, replacement) => current.Replace(replacement, string.Empty, StringComparison.OrdinalIgnoreCase)).Trim();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{PluginName}] attempting to process [{MetaTag}]", DisplayName, metaTag);
        }

        var wasTagValueModified = !string.Equals(newReleaseTitle, releaseTitle, StringComparison.OrdinalIgnoreCase);
        var result = new List<MetaTag<object?>>
        {
            new()
            {
                Identifier = metaTag.Identifier,
                Value = newReleaseTitle,
                OriginalValue = wasTagValueModified ? metaTag.Value : null
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(Artist)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
