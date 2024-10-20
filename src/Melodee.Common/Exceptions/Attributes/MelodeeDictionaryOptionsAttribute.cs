namespace Melodee.Common.Exceptions.Attributes;

public class MelodeeDictionaryOptionsAttribute(bool include, string? key = null) : Attribute
{
    public bool Ignore { get; } = !include;

    public string? Key { get; } = key;
}
