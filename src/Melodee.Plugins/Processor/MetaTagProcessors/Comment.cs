using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Removes any Comments.
/// </summary>
public sealed class Comment(Dictionary<string, object?> configuration, ISerializer serializer) : MetaTagProcessorBase(configuration, serializer)
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
            new()
            {
                Identifier = MetaTagIdentifier.Comment,
                Value = null,
                OriginalValue = metaTag.Value?.ToString().Nullify() == null ? null : metaTag.Value
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(Comment)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
