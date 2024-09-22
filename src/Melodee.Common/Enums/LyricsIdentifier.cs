namespace Melodee.Common.Enums;

public enum LyricsIdentifier
{
    /// <summary>
    ///     Other (i.e. none of the other types of this enum)
    /// </summary>
    Other = 0,

    /// <summary>
    ///     Lyrical data
    /// </summary>
    Lyrics = 1,

    /// <summary>
    ///     Transcription
    /// </summary>
    Transcription = 2,

    /// <summary>
    ///     List of the movements in the piece
    /// </summary>
    MovementName = 3,

    /// <summary>
    ///     Events that occur
    /// </summary>
    Event = 4,

    /// <summary>
    ///     Chord changes that occur in the music
    /// </summary>
    Chord = 5,

    /// <summary>
    ///     Trivia or "pop up" information about the media
    /// </summary>
    Trivia = 6,

    /// <summary>
    ///     URLs for relevant webpages
    /// </summary>
    WebpageUrl = 7,

    /// <summary>
    ///     URLs for relevant images
    /// </summary>
    ImageUrl = 8
}
