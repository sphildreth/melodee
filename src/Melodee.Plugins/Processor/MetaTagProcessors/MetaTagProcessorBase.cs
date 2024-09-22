using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

public abstract class MetaTagProcessorBase(Configuration configuration) : IMetaTagProcessor
{
    protected Configuration Configuration { get; } = configuration;

    public abstract string Id { get; }

    public abstract string DisplayName { get; }

    public virtual bool IsEnabled { get; set; } = true;

    public abstract int SortOrder { get; }

    public abstract bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier);

    public abstract OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags);
}
