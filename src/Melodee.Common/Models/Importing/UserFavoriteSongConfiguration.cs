namespace Melodee.Common.Models.Importing;

public record UserFavoriteSongConfiguration(string CsvFileName, Guid UserApiKey, string ArtistColumn, string AlbumColumn, string SongColumn, bool IsPretend);
