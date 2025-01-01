using Melodee.Common.Models;

namespace Melodee.Common.Plugins.Processor;

public interface IMetaTagsProcessorPlugin : IPlugin
{
    Task<OperationResult<IEnumerable<MetaTag<object?>>>> ProcessMetaTagAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, in IEnumerable<MetaTag<object?>> metaTags, CancellationToken cancellationToken = default);
}
