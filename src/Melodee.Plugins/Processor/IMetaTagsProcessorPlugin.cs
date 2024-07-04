using Melodee.Common.Models;

namespace Melodee.Plugins.Processor;

public interface IMetaTagsProcessorPlugin : IPlugin
{
    Task<OperationResult<IEnumerable<MetaTag<object?>>>> ProcessMetaTagAsync(IEnumerable<MetaTag<object?>> metaTags, CancellationToken cancellationToken = default);
}