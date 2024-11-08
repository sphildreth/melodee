namespace Melodee.Common.Models.Validation;

public enum ValidationResultMessageSeverity
{
    NotSet = 0,

    /// <summary>
    /// Something that should be fixed, likely doesn't adhere to any ID3 standard.
    /// </summary>
    Undesired,

    /// <summary>
    /// Something that must be fixed.
    /// </summary>
    Critical
}
