using System.Diagnostics;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.Melodee.DTOs;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using ServiceStack;

namespace Melodee.Common.Plugins.SearchEngine;

/// <summary>
/// Searches for Artist using the Melodee database
/// </summary>
public class MelodeeArtistSearchEnginPlugin(IDbContextFactory<MelodeeDbContext> contextFactory)
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "018A798D-7B68-4F3E-80CD-1BAF03998C0B";

    public string DisplayName => "Melodee Database";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var startTicks = Stopwatch.GetTimestamp();
            var data = new List<ArtistSearchResult>();

            if (query.MusicBrainzId != null)
            {
                var artistByMusicBrainz = await scopedContext.Artists
                    .Select(x => new { x.Id, x.Name, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName, x.AlbumCount, x.AlternateNames })
                    .FirstOrDefaultAsync(x => x.MusicBrainzId == query.MusicBrainzIdValue, cancellationToken)
                    .ConfigureAwait(false);

                if (artistByMusicBrainz != null)
                {
                    var artistAlbums = await scopedContext
                        .Albums
                        .Where(x => x.ArtistId == artistByMusicBrainz.Id)
                        .ToArrayAsync(cancellationToken)
                        .ConfigureAwait(false);
                    data.Add(new ArtistSearchResult
                    {
                        ApiKey = artistByMusicBrainz.ApiKey,
                        AlternateNames = artistByMusicBrainz.AlternateNames?.ToTags()?.ToArray() ?? [],
                        FromPlugin = DisplayName,
                        UniqueId = SafeParser.Hash(artistByMusicBrainz.MusicBrainzId.ToString()),
                        Rank = short.MaxValue,
                        Name = artistByMusicBrainz.Name,
                        SortName = artistByMusicBrainz.SortName,
                        MusicBrainzId = artistByMusicBrainz.MusicBrainzId,
                        AlbumCount = artistByMusicBrainz.AlbumCount,
                        Releases = artistAlbums.OrderBy(x => x.ReleaseDate).ThenBy(x => x.SortName).Select(x => new AlbumSearchResult
                        {
                            ApiKey = x.ApiKey,
                            AlbumType = x.AlbumTypeValue,
                            ReleaseDate = x.ReleaseDate.ToString(),
                            UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                            Name = x.Name,
                            NameNormalized = x.NameNormalized,
                            SortName = x.SortName ?? x.Name,
                            MusicBrainzId = x.MusicBrainzId
                        }).ToArray()
                    });
                }
            }

            // Return first artist that matches and has album that matches any of the album names - the more matches ranks higher
            if (data.Count == 0 && query.AlbumKeyValues?.Length > 0)
            {
                var artistsByNamedNormalizedWithMatchingAlbums = await scopedContext.Artists
                    .Select(x => new
                    {
                        x.Id, x.Name, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName, x.AlbumCount, x.AlternateNames, x.NameNormalized,
                        Albums = x.Albums.Select(a => new { a.AlbumType, a.AlternateNames, a.ReleaseDate, a.MusicBrainzId, a.NameNormalized, a.Name, a.SortName, a.ApiKey })
                    })
                    .Where(x => x.NameNormalized == query.NameNormalized || (x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)))
                    .OrderBy(x => x.SortName)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (artistsByNamedNormalizedWithMatchingAlbums.Length > 0)
                {
                    var matchingWithAlbums = artistsByNamedNormalizedWithMatchingAlbums
                        .Where(x => query.AlbumNamesNormalized != null &&
                                    (x.Albums.Any(a => query.AlbumNamesNormalized.Contains(a.NameNormalized)) ||
                                     x.Albums.Any(a => a.AlternateNames != null && a.AlternateNames.ContainsAny(query.AlbumNamesNormalized))))
                        .ToArray();

                    if (matchingWithAlbums.Length != 0)
                    {
                        data.AddRange(matchingWithAlbums.Select(x => new ArtistSearchResult
                        {
                            ApiKey = x.ApiKey,
                            AlternateNames = x.AlternateNames?.ToTags()?.ToArray() ?? [],
                            FromPlugin = DisplayName,
                            UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                            Rank = matchingWithAlbums.Length + 5,
                            Name = x.Name,
                            SortName = x.SortName,
                            MusicBrainzId = x.MusicBrainzId,
                            AlbumCount = x.AlbumCount,
                            Releases = x.Albums.Where(a => query.AlbumNamesNormalized != null && (query.AlbumNamesNormalized.Contains(a.NameNormalized) || (a.AlternateNames != null && a.AlternateNames.ContainsAny(query.AlbumNamesNormalized)))).OrderBy(a => a.ReleaseDate).ThenBy(a => a.SortName).Select(a => new AlbumSearchResult
                            {
                                ApiKey = a.ApiKey,
                                AlbumType = SafeParser.ToEnum<AlbumType>(a.AlbumType),
                                ReleaseDate = a.ReleaseDate.ToString(),
                                UniqueId = SafeParser.Hash(a.MusicBrainzId.ToString()),
                                Name = a.Name,
                                NameNormalized = a.NameNormalized,
                                SortName = a.SortName ?? x.Name,
                                MusicBrainzId = a.MusicBrainzId
                            }).ToArray()
                        }));
                    }
                }
            }

            if (data.Count == 0)
            {
                var artistsByNamedNormalized = await scopedContext.Artists
                    .Select(x => new
                    {
                        x.Id, x.Name, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName, x.AlbumCount, x.AlternateNames, x.NameNormalized,
                        Albums = x.Albums.Select(a => new { a.AlbumType, a.AlternateNames, a.ReleaseDate, a.MusicBrainzId, a.NameNormalized, a.Name, a.SortName, a.ApiKey })
                    })
                    .Where(x => x.NameNormalized == query.NameNormalized || (x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)))
                    .OrderBy(x => x.SortName)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (artistsByNamedNormalized.Length > 0)
                {
                    data.AddRange(artistsByNamedNormalized.Select(x => new ArtistSearchResult
                    {
                        ApiKey = x.ApiKey,
                        AlternateNames = x.AlternateNames?.ToTags()?.ToArray() ?? [],
                        FromPlugin = DisplayName,
                        UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                        Rank = 1,
                        Name = x.Name,
                        SortName = x.SortName,
                        MusicBrainzId = x.MusicBrainzId,
                        AlbumCount = x.AlbumCount,
                        Releases = x.Albums.OrderBy(a => a.ReleaseDate).ThenBy(a => a.SortName).Select(a => new AlbumSearchResult
                        {
                            ApiKey = a.ApiKey,
                            AlbumType = SafeParser.ToEnum<AlbumType>(a.AlbumType),
                            ReleaseDate = a.ReleaseDate.ToString(),
                            UniqueId = SafeParser.Hash(a.MusicBrainzId.ToString()),
                            Name = a.Name,
                            NameNormalized = a.NameNormalized,
                            SortName = a.SortName ?? x.Name,
                            MusicBrainzId = a.MusicBrainzId
                        }).ToArray()
                    }));
                }
            }

            return new PagedResult<ArtistSearchResult>
            {
                OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
                TotalCount = 0,
                TotalPages = 0,
                Data = data
            };
        }
    }

    public async Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(int forArtist, int maxResults, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();

            var startTicks = Stopwatch.GetTimestamp();
            SongSearchResult[] data = [];

            var artist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == forArtist, cancellationToken).ConfigureAwait(false);
            if (artist != null)
            {
                var sql = """
                          select ROW_NUMBER () OVER (
                            ORDER BY
                              s."PlayedCount" desc, s."LastPlayedAt" desc, s."SortOrder", s."TitleSort", a."SortOrder"
                          ) as "Index", s.*
                          from "Songs" s
                          join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                          join "Albums" a on (ad."AlbumId" = a."Id")
                          join "Artists" aa on (a."ArtistId" = aa."Id")
                          where aa."Id" = @artistId
                          order by s."PlayedCount" desc, s."LastPlayedAt" desc, s."SortOrder", s."TitleSort", a."SortOrder"
                          offset 0 rows fetch next @maxResults rows only;
                          """;

                var songs = (await dbConn
                    .QueryAsync<TopSongSearch>(sql, new { artistId = artist.Id, maxResults })
                    .ConfigureAwait(false)).ToArray();

                data = songs.Select(x => x.ToSearchEngineSongSearchResult(x.Index)).ToArray();
            }

            return new PagedResult<SongSearchResult>
            {
                OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
                TotalCount = 0,
                TotalPages = 1,
                Data = data
            };
        }
    }
}
