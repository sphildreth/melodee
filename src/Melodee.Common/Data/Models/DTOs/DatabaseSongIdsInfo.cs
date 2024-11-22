namespace Melodee.Common.Data.Models.DTOs;

/// <summary>
/// Used to get aggregate data for a given SongId
/// </summary>
public record DatabaseSongIdsInfo(int SongId, Guid SongApiKey, int AlbumDiscId, int AlbumId, Guid AlbumApiKey, int AlbumArtistId, Guid AlbumArtistApiKey);

