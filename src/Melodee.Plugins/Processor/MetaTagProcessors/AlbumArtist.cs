using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the Album Artist and split away any featuring artists.
/// </summary>
public sealed partial class AlbumArtist(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "29D61BF9-D283-4DB6-B7EB-16F6BCA76998";

    public override string DisplayName => nameof(Artist);

    public override int SortOrder { get; } = 1;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier is MetaTagIdentifier.AlbumArtist;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        var albumArtist = tagValue as string ?? string.Empty;
        var result = new List<MetaTag<object?>>();
        
        if (albumArtist.Nullify() != null)
        {
            if (Configuration.PluginProcessOptions.ArtistNameReplacements.Any())
            {
                foreach (var kp in Configuration.PluginProcessOptions.ArtistNameReplacements)
                {
                    if (kp.Value.Any(kpv => string.Equals(albumArtist, kpv)))
                    {
                        albumArtist = kp.Key;
                        break;
                    }
                }
            }
            if (albumArtist.HasFeaturingFragments())
            {
                albumArtist = AlbumArtistFromAlbumArtistViaFeaturing(metaTag.Value?.ToString() ?? string.Empty);
            }

            if (albumArtist != null)
            {
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = albumArtist,
                    OriginalValue = metaTag.Value
                });
            }             
        }
        result.ForEach(x => x.AddProcessedBy(nameof(Artist)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
