using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Removes any Comments.
/// </summary>
public sealed class Comment(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "1CC2FAE8-AA28-4D03-8FBF-2FD42F080195";

    public override string DisplayName => nameof(Comment);

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.Comment;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var result = new List<MetaTag<object?>>
        {
            new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.Comment,
                Value = null,
                OriginalValue = metaTag.Value?.ToString().Nullify() == null ? null : metaTag.Value
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(Artist)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
