using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models;

/// <summary>
/// This is a representation of a single MetaDat Tag (like 'TRCK') and its value.
/// </summary>
/// <typeparam name="T">Data type of tag.</typeparam>
[Serializable]
public sealed record MetaTag<T>
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required MetaTagIdentifier Identifier { get; init; }

    public string IdentifierDescription => $"{Identifier.ToString()} {Identifier.GetEnumDescriptionValue()}";
    
    public required T Value { get; init; }
    
    public T? OriginalValue { get; init; }
    
    public int SortOrder { get; set; }
    
    [JsonIgnore]
    public bool WasModified => OriginalValue != null;

    public StyleClass StyleClass { get; set; } = StyleClass.Normal;
}
