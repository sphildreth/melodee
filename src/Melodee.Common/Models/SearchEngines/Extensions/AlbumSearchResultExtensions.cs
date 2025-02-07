using Melodee.Common.Enums;
using Melodee.Common.Models.Collection;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Models.SearchEngines.Extensions;

public static class AlbumSearchResultExtensions
{
    public static AlbumDataInfo ToAlbumDataInfo(this AlbumSearchResult searchResult)
    {
        return new AlbumDataInfo
        (
            searchResult.Id ?? 0,
            searchResult.ApiKey ?? Guid.Empty,
            false,
            searchResult.Name,
            searchResult.NameNormalized,
            null,
            Guid.Empty,
            searchResult.Artist?.Name ?? string.Empty,
            0,
            0,
            Instant.FromDateTimeUtc(DateTime.UtcNow),
            null,
            LocalDate.FromDateTime(SafeParser.ToDateTime(searchResult.ReleaseDate) ?? DateTime.MinValue),
            (short)AlbumStatus.Ok
        )
        {
            CoverUrl = searchResult.CoverUrl,
            AlbumSearchResult = searchResult
        };
    }
}
