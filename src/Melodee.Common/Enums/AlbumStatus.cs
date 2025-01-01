namespace Melodee.Common.Enums;

public enum AlbumStatus
{
    NotSet = 0,

    /// <summary>
    ///     Ready to be moved to Library
    /// </summary>
    Ok,

    /// <summary>
    ///     Not seen by a reviewer.
    /// </summary>
    New,

    /// <summary>
    ///     Needs some attention as it has issues and is not 'Ok'.
    /// </summary>
    Invalid
}
