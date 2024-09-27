using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

public abstract partial class MetaTagProcessorBase(Configuration configuration) : IMetaTagProcessor
{
    protected Configuration Configuration { get; } = configuration;

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public virtual bool IsEnabled { get; set; } = true;

    public abstract int SortOrder { get; }

    public abstract bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier);

    public abstract OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags);

    protected static string? ReleaseArtistFromReleaseArtistViaFeaturing(string artist)
    {
        var newArtist = artist;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(artist);
        return newArtist[..matches.Index].CleanString();
    }
    
    protected static string? TrackArtistFromTitleViaFeaturing(string title)
    {
        var newTitle = title;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(title);
        var result = newTitle.Substring(matches.Index + matches.Length).CleanString();
        Regex rgx = RemoveNonAlphaNumbericRegex();
        return rgx.Replace(result ?? string.Empty, string.Empty).Nullify();
    }    

    protected static string? TrackArtistFromReleaseArtistViaFeaturing(string? artist)
    {
        if (string.IsNullOrWhiteSpace(artist))
        {
            return null;
        }
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(artist);
        var featureArtist = ReplaceArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(artist.Substring(matches.Index), string.Empty).CleanString());
        return featureArtist?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");        
    }

    protected static string? TrackTitleWithoutFeaturingArtist(string? title)
    {
        var newTitle = title ?? string.Empty;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(title!);
        if (matches.Index > 0)
        {
            return newTitle[..matches.Index].CleanString();
        }
        return title;
    }
    
    private static string? ReplaceArtistSeparators(string? trackArtist)
    {
        return trackArtist.Nullify() == null ? null : Artist.ReplaceArtistSeparatorsRegex().Replace(trackArtist!, "/").Trim();
    }

    [GeneratedRegex("[^a-zA-Z0-9 -]")]
    private static partial Regex RemoveNonAlphaNumbericRegex();
}
