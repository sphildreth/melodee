namespace Melodee.Common.Models.OpenSubsonic;

public record Playlist
{
    /// <summary>
    ///     Id of the playlist
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     Name of the playlist
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     A commnet
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    ///     Owner of the playlist
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    ///     Is the playlist public
    /// </summary>
    public bool Public { get; init; }

    /// <summary>
    ///     number of songs
    /// </summary>
    public required int SongCount { get; init; }

    /// <summary>
    ///     Playlist duration in seconds
    /// </summary>
    public required int Duration { get; init; }

    /// <summary>
    ///     Creation date [ISO 8601]
    /// </summary>
    public required string Created { get; init; }

    /// <summary>
    ///     Last changed date [ISO 8601]
    /// </summary>
    public required string Changed { get; init; }

    /// <summary>
    ///     A cover Art Id
    /// </summary>
    public string? CoverArt { get; init; }

    /// <summary>
    ///     A list of allowed usernames
    /// </summary>
    public string[]? AllowedUsers { get; init; }

    /// <summary>
    ///     Songs on the playlist.
    /// </summary>
    public Child[]? Entry { get; init; }
}
