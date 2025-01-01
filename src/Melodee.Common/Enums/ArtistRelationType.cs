namespace Melodee.Common.Enums;

public enum ArtistRelationType
{
    NotSet = 0,

    /// <summary>
    ///     A band can be considered similar to another if they share significant musical elements
    ///     like genre, tempo, instrumentation, lyrical themes, vocal style, song structure, production
    ///     techniques, and even a similar overall aesthetic or vibe, essentially creating a
    ///     recognizable sonic likeness between the two acts.
    /// </summary>
    Similar,

    /// <summary>
    ///     An artist is associated with another; can be a founder, an active member, a past member, etc.
    ///     <remarks>
    ///         This includes MusicBrainz "Member of Band" 5be4c609-9afa-4ea0-910b-12ffb71e3821
    ///         This includes MusicBrainz "Subgroup" 7802f96b-d995-4ce9-8f70-6366faad758e
    ///         This includes MusicBrainz "Founder" 6ed4bfc4-0a0d-44c0-b025-b7fc4d900b67
    ///     </remarks>
    /// </summary>
    Associated
}
