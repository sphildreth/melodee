namespace Melodee.Common.Models.OpenSubsonic.Enums;

public enum ListType
{
    NotSet = 0,

    Random,

    /// <summary>
    ///     Ordered by recently added desc
    /// </summary>
    Newest,

    /// <summary>
    ///     Ordered by top rating desc
    /// </summary>
    Highest,

    /// <summary>
    ///     Ordered by most played
    /// </summary>
    Frequent,

    /// <summary>
    ///     Ordered by most recently played
    /// </summary>
    Recent,

    AlphabeticalByName,

    AlphabeticalByArtist,

    Starred,

    ByYear,

    ByGenre
}
