namespace Melodee.Common.Enums;

/// <summary>
///     Is this MetaTag Identifier allowed to have multiple values. Multiple values will be seperated by a single forward
///     slash (/).
/// </summary>
public sealed class MetaTagMultiValueAttribute(bool isMultiValueAllowed) : Attribute
{
    public bool IsMultiValueAllowed { get; } = isMultiValueAllowed;
}
