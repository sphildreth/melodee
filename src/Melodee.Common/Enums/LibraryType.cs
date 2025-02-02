namespace Melodee.Common.Enums;

public enum LibraryType
{
    NotSet = 0,

    /// <summary>
    ///     Inbound holds metadata to get processed, should be 1.
    /// </summary>
    Inbound,

    /// <summary>
    ///     Processed metadata into Melodee Albums, should be 1
    /// </summary>
    Staging,

    /// <summary>
    ///     Storage library used by API, can be many
    /// </summary>
    Storage,

    /// <summary>
    ///     User images library, should be 1
    /// </summary>
    UserImages
}
