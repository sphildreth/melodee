using System.Text.RegularExpressions;
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
        var configuration1 = configuration;
        _metaTagProcessors = new IMetaTagProcessor[]
        {
            new Album(configuration1),
            new Artist(configuration1),
            new Comment(configuration1),
            new OrigReleaseYear(configuration1),
            new ReleaseArtist(configuration1),            
            new TrackTitle(configuration1),
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
                foreach (var tag in processedTags.ToArray())
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
