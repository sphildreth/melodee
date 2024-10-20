using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Plugins.Processor.MetaTagProcessors;

/// <summary>
///     Ensures OrigReleaseYear is set, if not tries to find it from directory title, if not sets to default.
/// </summary>
public sealed class OrigReleaseYear(Configuration configuration) : MetaTagProcessorBase(configuration)
{
    public override string Id => "652676F9-3BCA-48D2-8473-C7CAE28E0020";

    public override string DisplayName => nameof(OrigReleaseYear);

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleMetaTagIdentifier(MetaTagIdentifier metaTagIdentifier)
    {
        return metaTagIdentifier == MetaTagIdentifier.OrigReleaseYear;
    }

    public override OperationResult<IEnumerable<MetaTag<object?>>> ProcessMetaTag(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, MetaTag<object?> metaTag, in IEnumerable<MetaTag<object?>> metaTags)
    {
        var tagValue = metaTag.Value;
        var yearValue = SafeParser.ToNumber<int>(tagValue ?? string.Empty);
        if (yearValue < Configuration.PluginProcessOptions.MinimumValidReleaseYear)
        {
            yearValue = directoryInfo.FullName().TryToGetYearFromString() ?? fileSystemFileInfo.FullName(directoryInfo).TryToGetYearFromString() ?? 0;
            if (yearValue < Configuration.PluginProcessOptions.MinimumValidReleaseYear && Configuration.PluginProcessOptions.DoUseCurrentYearAsDefaultOrigReleaseYearValue)
            {
                yearValue = DateTime.UtcNow.Year;
                Log.Debug("Used current year for OrigReleaseYear.");
            }
        }

        var result = new List<MetaTag<object?>>
        {
            new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.OrigReleaseYear,
                Value = yearValue
            }
        };
        result.ForEach(x => x.AddProcessedBy(nameof(Artist)));       
        return new OperationResult<IEnumerable<MetaTag<object?>>>
        {
            Data = result
        };
    }
}
