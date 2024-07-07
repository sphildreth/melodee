using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor.MetaTagProcessors;

namespace Melodee.Plugins.Processor;

public sealed class MetaTagsProcessor : IMetaTagsProcessorPlugin
{
    private readonly Configuration _configuration;
    private IEnumerable<IMetaTagProcessor> _metaTagProcessors;
    
    public string Id => "EBFFDB54-F24E-42F3-B98F-6C65500249FE";

    public string DisplayName => nameof(MetaTagsProcessor);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public MetaTagsProcessor(Configuration configuration)
    {
        _configuration = configuration;
        _metaTagProcessors = new IMetaTagProcessor[]
        {
            new TrackTitle(_configuration),
            new Album(_configuration),
            new Artist(_configuration),
            new Comment(_configuration),
        };
    }
    
    public async Task<OperationResult<IEnumerable<MetaTag<object?>>>> ProcessMetaTagAsync(IEnumerable<MetaTag<object?>> metaTags, CancellationToken cancellationToken = default)
    {
        var processedTags = new List<MetaTag<object?>>(metaTags.ToList());
        foreach (var metaTagProcessor in _metaTagProcessors)
        {
            if (metaTagProcessor.IsEnabled)
            {
                foreach (var tag in processedTags.ToArray())
                {
                    if (metaTagProcessor.DoesHandleMetaTagIdentifier(tag.Identifier))
                    {
                        var metaTagProcessorResult = metaTagProcessor.ProcessMetaTag(tag, processedTags);
                        if (metaTagProcessorResult.IsSuccess)
                        {
                            foreach (var processorResultTag in metaTagProcessorResult.Data)
                            {
                                processedTags.RemoveAll(x => x.Identifier == processorResultTag.Identifier);
                                if (processorResultTag.Value != null)
                                {
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
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = processedTags.ToArray()
        };
    }
}