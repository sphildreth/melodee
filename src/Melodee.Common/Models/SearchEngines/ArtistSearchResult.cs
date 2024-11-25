
namespace Melodee.Common.Models.SearchEngines;

public record ArtistSearchResult(long UniqueId, string Name, string RealName, string SortName, string ThumbnailUrl, ReleaseSearchResult[] Releases);
