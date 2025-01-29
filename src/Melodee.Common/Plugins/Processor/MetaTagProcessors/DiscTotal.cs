using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;

namespace Melodee.Common.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Ensures DiscTotal is set, if not tries to parse it from DiscTotalNumber, if not sets to default (1).
/// </summary>
public sealed class DiscTotal(Dictionary<string, object?> configuration, ISerializer serializer) : MetaTagProcessorBase(configuration, serializer)
{
    public override string Id => "4A3CB4E0-8D38-4DDE-AFC3-DC6B796D4CDD";

    public override string DisplayName => nameof(DiscTotal);

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier is MetaTagIdentifier.DiscTotal or MetaTagIdentifier.DiscNumberTotal;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        var discTotalValue = SafeParser.ToNumber<short?>(tagValue ?? string.Empty);
        if (discTotalValue == 0)
        {
            var discTotalToParse = metaTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.DiscNumberTotal);
            if (discTotalToParse != null)
            {
                var discTotalParts = discTotalToParse.Value?.ToString()?.Split('/') ?? [];
                if (discTotalParts.Length > 1)
                {
                    discTotalValue = SafeParser.ToNumber<short?>(discTotalParts[1]);
                }
                else
                {
                    discTotalValue = SafeParser.ToNumber<short?>(discTotalToParse);
                }
            }
            else
            {
                discTotalValue = metaTags
                    .Where(x => x.Identifier == MetaTagIdentifier.DiscNumber)
                    .Select(x => x.Value)
                    .Where(x => x != null).Select(SafeParser.ToNumber<short?>)
                    .Max();
            }
        }

        if (discTotalValue < 1)
        {
            discTotalValue = 1;
        }

        var result = new List<MetaTag<object?>>
        {
            new()
            {
                Identifier = MetaTagIdentifier.DiscTotal,
                Value = discTotalValue ?? 1
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(DiscTotal)));
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
