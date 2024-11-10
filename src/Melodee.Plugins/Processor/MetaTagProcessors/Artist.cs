using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Handle the Song Artist and split away any featuring artists.
/// </summary>
public sealed class Artist(Dictionary<string, object?> configuration, ISerializer serializer) : MetaTagProcessorBase(configuration, serializer)
{
    public override string Id => "29D61BF9-D283-4DB6-B7EB-16F6BCA76998";

    public override string DisplayName => nameof(Artist);

    public override int SortOrder { get; } = 1;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier is MetaTagIdentifier.Artist;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        var artist = tagValue as string ?? string.Empty;

        var result = new List<MetaTag<object?>>();

        if (artist.Nullify() != null)
        {
            var artistNameReplacements = MelodeeConfiguration.FromSerializedJsonDictionary(Configuration[SettingRegistry.ProcessingArtistNameReplacements], Serializer);
            if (artistNameReplacements.Any())
            {
                foreach (var kp in artistNameReplacements)
                {
                    if (kp.Value.Any(kpv => string.Equals(artist, kpv)))
                    {
                        artist = kp.Key;
                        break;
                    }
                }
            }

            var metaTagsValue = metaTags?.ToArray() ?? [];

            // See if the artist has featuring artists
            if (artist.Nullify() != null && artist.HasFeaturingFragments())
            {
                // Get the Song artist from the artist and modify the artist
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Artist,
                    Value = SongArtistFromAlbumArtistViaFeaturing(artist),
                    OriginalValue = artist
                });
                if (metaTagsValue?.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist) ?? false)
                {
                    result.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.AlbumArtist,
                        Value = AlbumArtistFromAlbumArtistViaFeaturing(metaTag.Value?.ToString() ?? string.Empty),
                        OriginalValue = artist
                    });
                }
            }

            // See if the Title has featuring artists
            var title = metaTagsValue?.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value as string;
            string? songArtist = null;
            if (title.Nullify() != null && title.HasFeaturingFragments())
            {
                // Get the Song artist from the title and modify the title
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Title,
                    Value = SongTitleWithoutFeaturingArtist(title),
                    OriginalValue = title
                });
                // Add the Song artist from title 
                songArtist = SongArtistFromTitleViaFeaturing(title!);
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Artist,
                    Value = artist.FeaturingAndWithFragmentsCount() > 1 ? artist : songArtist
                });

                // Ensure the AlbumArtist is set
                if (metaTagsValue?.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist) ?? false)
                {
                    result.Add(new MetaTag<object?>
                    {
                        Identifier = MetaTagIdentifier.AlbumArtist,
                        Value = artist,
                        OriginalValue = metaTag.Value
                    });
                }
            }

            if (artist != null && result.All(x => x.Identifier != MetaTagIdentifier.Artist))
            {
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Artist,
                    Value = artist,
                    OriginalValue = metaTag.Value
                });
            }

            // If the value for the "Artist" (Song artist) matches the "AlbumArtist" (Album artist) then nullify the "Artist" value.
            var albumArtist = (result.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist) ?? metaTagsValue?.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist))?.Value as string;
            songArtist = (result.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist) ?? metaTagsValue?.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist))?.Value as string;
            if (songArtist.Nullify() != null && songArtist!.DoStringsMatch(albumArtist))
            {
                result.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Artist,
                    Value = null,
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
