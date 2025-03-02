using Melodee.Common.Filtering;
using Melodee.Common.Utility;
using Radzen;
using FilterOperator = Melodee.Common.Filtering.FilterOperator;

namespace Melodee.Blazor.Extensions;

public static class FilterDescriptorExtensions
{
    public static FilterOperatorInfo ToFilterOperatorInfo(this FilterDescriptor descriptor)
    {
        var filterOperator = SafeParser.ToEnum<FilterOperator>(descriptor.FilterOperator.ToString());
        return new FilterOperatorInfo(descriptor.Property, filterOperator, descriptor.FilterValue);
    }
}
