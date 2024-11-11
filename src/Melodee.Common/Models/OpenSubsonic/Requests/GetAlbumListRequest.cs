using Melodee.Common.Models.OpenSubsonic.Enums;

namespace Melodee.Common.Models.OpenSubsonic.Requests;

/// <summary>
/// 
/// </summary>
/// <param name="Type">The list type.</param>
/// <param name="Size">The number of albums to return. Max 500.</param>
/// <param name="Offset">The list offset. Useful if you for example want to page through the list of newest albums.</param>
/// <param name="FromYear">The first year in the range. If fromYear > toYear a reverse chronological list is returned.</param>
/// <param name="ToYear">The last year in the range.</param>
/// <param name="Genre">The name of the genre, e.g., “Rock”.</param>
/// <param name="MusicFolderId">Only return albums in the music folder with the given ID. </param>
public record GetAlbumListRequest(ListType Type, int? Size, int? Offset, int? FromYear, int? ToYear, string? Genre, string? MusicFolderId);
