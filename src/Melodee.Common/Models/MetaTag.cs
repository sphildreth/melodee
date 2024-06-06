using Melodee.Common.Enums;

namespace Melodee.Common.Models;

public sealed record MetaTag<T>
{
    public required MetaTagIdentifier Identifier { get; init; }
    
    public required T Value { get; init; }
    
    public int SortOrder { get; init; }
}