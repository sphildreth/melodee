namespace Melodee.Common.Enums;

public enum ContributorType
{
    NotSet = 0,

    /// <summary>
    ///     Someone who performed on the song, be that a singer, a guitarist, etc.
    /// </summary>
    Performer,

    /// <summary>
    ///     Someone involved in the production of the song. Producer, Mixer, Composer, etc.
    /// </summary>
    Production,

    /// <summary>
    ///     Someone (or thing) that helped get the song published.
    /// </summary>
    Publisher
}

