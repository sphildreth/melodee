using NodaTime;

namespace Melodee.Common.Models.SearchEngines;

public record AlbumSearchResult(long UniqueId, ArtistSearchResult Artist, Instant ReleaseDate, string[] Genres, string ThumbnailUrl, string Name, string NameNormalized, string SortTitle);
