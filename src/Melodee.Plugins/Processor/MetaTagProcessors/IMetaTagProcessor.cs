using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

public interface IMetaTagProcessor : IPlugin
{
    bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier);
    
    OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(MetaTag<object?> metaTag, IEnumerable<MetaTag<object?>> metaTags);
}