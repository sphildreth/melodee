using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Processor.MetaTagProcessors;

namespace Melodee.Plugins.Processor;

public sealed partial class MetaTagsProcessor : IMetaTagsProcessorPlugin
{
    private readonly IEnumerable<IMetaTagProcessor> _metaTagProcessors;

    public MetaTagsProcessor(Configuration configuration)
    {
        _metaTagProcessors = new IMetaTagProcessor[]
        {
            new Album(configuration),
            new ReleaseArtist(configuration),            
            new Artist(configuration),
            new Comment(configuration),
            new OrigReleaseYear(configuration),
            new TrackTitle(configuration),
        };
    }

    public string Id => "EBFFDB54-F24E-42F3-B98F-6C65500249FE";

    public string DisplayName => nameof(MetaTagsProcessor);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public Task<OperationResult<IEnumerable<MetaTag<object?>>>> ProcessMetaTagAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, in IEnumerable<MetaTag<object?>> metaTags, CancellationToken cancellationToken = default)
    {
        var processedTags = new List<MetaTag<object?>>(metaTags.ToList());
        foreach (var metaTagProcessor in _metaTagProcessors.OrderBy(x => x.SortOrder))
        {
            if (metaTagProcessor.IsEnabled)
            {
                foreach (var tag in processedTags.OrderBy(x => x.SortOrder).ToArray())
                {
                    if (metaTagProcessor.DoesHandleMetaTagIdentifier(tag.Identifier))
                    {
                        var metaTagProcessorResult = metaTagProcessor.ProcessMetaTag(directoryInfo, fileSystemFileInfo, tag, processedTags);
                        if (metaTagProcessorResult.IsSuccess)
                        {
                            foreach (var processorResultTag in metaTagProcessorResult.Data)
                            {
                                processedTags.RemoveAll(x => x.Identifier == processorResultTag.Identifier);
                                if (processorResultTag.Value != null)
                                {
                                    processorResultTag.AddProcessedBy(metaTagProcessor.DisplayName);
                                    processedTags.Add(processorResultTag);
                                }
                            }
                        }
                        else
                        {
                            processedTags.RemoveAll(x => x.Identifier == tag.Identifier);
                        }
                    }
                }
            }
        }
        // Ensure that album artist is set
        if (processedTags.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist))
        {
            var groupedTags = processedTags.GroupBy(x => x.Identifier);
            var artistTag = groupedTags.Where(x => x.Key == MetaTagIdentifier.Artist).OrderByDescending(x => x.Count()).FirstOrDefault();
            if (artistTag != null)
            {
                processedTags.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = artistTag.FirstOrDefault()?.Value,
                    OriginalValue = artistTag.FirstOrDefault()?.OriginalValue
                });
            }
        }
        return Task.FromResult(new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = processedTags.ToArray()
        });
    }


    public static string? ReplaceTrackArtistSeparators(string? trackArtist)
    {
        return trackArtist.Nullify() == null ? null : ReplaceTrackArtistSeparatorsRegex().Replace(trackArtist!, "/").Trim();
    }

    [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceTrackArtistSeparatorsRegex();
}
