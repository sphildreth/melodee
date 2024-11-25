using NodaTime;

namespace Melodee.Common.Models.SearchEngines;

public record ReleaseSearchResult(long UniqueId, ArtistSearchResult Artist, Instant ReleaseDate, string[] Genres, string ThumbnailUrl, string Name, string NameNormalized, string SortTitle);
