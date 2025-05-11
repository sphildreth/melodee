using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Serialization;

namespace Melodee.Common.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Ensures DiscTotal is set to 1, always.
/// </summary>
public sealed class DiscTotal(Dictionary<string, object?> configuration, ISerializer serializer)
    : MetaTagProcessorBase(configuration, serializer)
{
    public override string Id => "4A3CB4E0-8D38-4DDE-AFC3-DC6B796D4CDD";

    public override string DisplayName => nameof(DiscTotal);

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier is MetaTagIdentifier.DiscTotal or MetaTagIdentifier.DiscNumberTotal;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo,
        FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var result = new List<MetaTag<object?>>
        {
            new()
            {
                Identifier = MetaTagIdentifier.DiscTotal,
                Value = 1
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(DiscTotal)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Type = OperationResponseType.Ok,
            Data = result
        };
    }
}
