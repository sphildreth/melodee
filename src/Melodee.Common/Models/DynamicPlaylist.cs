using Newtonsoft.Json;

namespace Melodee.Common.Models;

/// <summary>
///     A definition of a dynamic playlist that uses the SongSelectionWhere to select songs to include in the playlist.
/// </summary>
/// <param name="Id">Unique id for the playlist.</param>
/// <param name="IsEnabled">Is this dynamic playlist enabled.</param>
/// <param name="Name">Name of playlist.</param>
/// <param name="Comment">Description of the playlist.</param>
/// <param name="IsPublic">Does this playlist appear for everyone or just the owner.</param>
/// <param name="ForUserId">When set this playlist is only visible to the user who matches this ApiKey.</param>
/// <param name="SongSelectionWhere">The WHERE part of the SQL to select songs for the playlist.</param>
/// <param name="SongSelectionOrder">The ORDER BY part of the SQL to select songs for the playlist, defaults to random.</param>
/// <param name="SongLimit">Limit songs returned to this number, overrides any paging from API client.</param>
[Serializable]
public sealed record DynamicPlaylist(
    Guid Id,
    bool IsEnabled,
    string Name,
    string Comment,
    bool IsPublic,
    Guid? ForUserId,
    string SongSelectionWhere,
    string? SongSelectionOrder = null,
    int? SongLimit = null)
{
    [JsonIgnore] public string? ImageFileName { get; set; }
}
