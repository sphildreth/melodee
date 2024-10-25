using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

public abstract partial class MetaTagProcessorBase(Dictionary<string, object?> configuration, ISerializer serializer) : IMetaTagProcessor
{
    protected Dictionary<string, object?> Configuration { get; } = configuration;

    protected ISerializer Serializer { get; } = serializer;
    
    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public virtual bool IsEnabled { get; set; } = true;

    public abstract int SortOrder { get; }

    public abstract bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier);

    public abstract OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags);

    protected static string? AlbumArtistFromAlbumArtistViaFeaturing(string artist)
    {
        var newArtist = artist;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(artist);
        return newArtist[..matches.Index].CleanString();
    }
    
    protected static string? SongArtistFromTitleViaFeaturing(string title)
    {
        var newTitle = title;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(title);
        var result = newTitle.Substring(matches.Index + matches.Length).CleanString();
        Regex rgx = RemoveNonAlphaNumbericRegex();
        return rgx.Replace(result ?? string.Empty, string.Empty).Nullify();
    }    

    protected static string? SongArtistFromAlbumArtistViaFeaturing(string? artist)
    {
        if (string.IsNullOrWhiteSpace(artist))
        {
            return null;
        }
        var result = artist;
        if (result.HasFeaturingFragments())
        {
            var matches = StringExtensions.HasFeatureFragmentsRegex.Match(result!);
            result = MetaTagsProcessor.ReplaceSongArtistSeparators(StringExtensions.HasFeatureFragmentsRegex.Replace(result!.Substring(matches.Index), string.Empty).CleanString());
        }
        if (result.HasWithFragments())
        {
            var matches = StringExtensions.HasWithFragmentsRegex.Match(result!);
            result = MetaTagsProcessor.ReplaceSongArtistSeparators(StringExtensions.HasWithFragmentsRegex.Replace(result!.Substring(matches.Index), string.Empty).CleanString());
        }         
        return result?.TrimEnd(']', ')').Replace("\"", "'").Replace("; ", "/").Replace(";", "/");
    }

    protected static string? SongTitleWithoutFeaturingArtist(string? title)
    {
        var newTitle = title ?? string.Empty;
        var matches = StringExtensions.HasFeatureFragmentsRegex.Match(title!);
        if (matches.Index > 0)
        {
            return newTitle[..matches.Index].CleanString();
        }
        return title;
    }
    
    protected static string? SongTitleWithoutWithArtist(string? title)
    {
        var newTitle = title ?? string.Empty;
        var matches = StringExtensions.HasWithFragmentsRegex.Match(title!);
        if (matches.Index > 0)
        {
            return newTitle[..matches.Index].CleanString();
        }
        return title;
    }    
    

    // [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    // internal static partial Regex ReplaceArtistSeparatorsRegex();    
    //
    // private static string? ReplaceArtistSeparators(string? SongArtist)
    // {
    //     return SongArtist.Nullify() == null ? null : Artist.ReplaceArtistSeparatorsRegex().Replace(SongArtist!, "/").Trim();
    // }

    [GeneratedRegex("[^a-zA-Z0-9 -]")]
    private static partial Regex RemoveNonAlphaNumbericRegex();
}
