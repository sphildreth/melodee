using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Common.Plugins.Processor.MetaTagProcessors;

public interface IMetaTagProcessor : IPlugin
{
    bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier);

    OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo,
        FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags);
}
