namespace Melodee.Common.Models.OpenSubsonic.Requests;

/// <summary>
///     Request to update a playlist.
/// </summary>
/// <param name="PlaylistId">The playlist ID.</param>
/// <param name="Name">The human-readable name of the playlist.</param>
/// <param name="Comment">The playlist comment.</param>
/// <param name="Public">true if the playlist should be visible to all users, false otherwise.</param>
/// <param name="SongIdToAdd">Add this song with this ID to the playlist. Multiple parameters allowed.</param>
/// <param name="SongIdToRemove">Remove the song at this position in the playlist. Multiple parameters allowed.</param>
public record UpdatePlayListRequest(string PlaylistId, string? Name, string? Comment, bool? Public, string[]? SongIdToAdd, string[]? SongIdToRemove);
