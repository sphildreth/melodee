namespace Melodee.Common.Models.OpenSubsonic;

public sealed record PlayQueue
{
    /// <summary>
    ///     This is the Id of the Child that is the currently playing song.
    /// </summary>
    public required string Current { get; init; }

    public required double Position { get; init; }

    public required string ChangedBy { get; init; }

    public required string Changed { get; init; }

    public required string Username { get; init; }

    public Child[]? Entry { get; init; }
}
