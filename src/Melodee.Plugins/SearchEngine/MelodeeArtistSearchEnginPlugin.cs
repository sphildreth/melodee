using System.Diagnostics;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.Melodee.DTOs;
using Microsoft.EntityFrameworkCore;
using Song = Melodee.Common.Data.Models.Song;

namespace Melodee.Plugins.SearchEngine;

public class MelodeeArtistSearchEnginPlugin(IDbContextFactory<MelodeeDbContext> contextFactory) 
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "018A798D-7B68-4F3E-80CD-1BAF03998C0B";

    public string DisplayName => "Melodee Database";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public async Task<PagedResult<ArtistSearchResult>> DoSearchAsync(IHttpClientFactory httpClientFactory, ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var startTicks = Stopwatch.GetTimestamp();
            var data = new List<ArtistSearchResult>();

            if (query.MusicBrainzId != null)
            {
                var artistByMusicBrainz = await scopedContext.Artists
                    .Select(x => new { x.Id, x.Name, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName })
                    .FirstOrDefaultAsync(x => x.MusicBrainzId == query.MusicBrainzIdValue, cancellationToken)
                    .ConfigureAwait(false);

                if (artistByMusicBrainz != null)
                {
                    data.Add(new ArtistSearchResult
                    {
                        ApiKey = artistByMusicBrainz.ApiKey,
                        Id = artistByMusicBrainz.Id,
                        FromPlugin = DisplayName,
                        MusicBrainzId = artistByMusicBrainz.MusicBrainzId,
                        Name = artistByMusicBrainz.Name,
                        Rank = byte.MaxValue,
                        SortName = artistByMusicBrainz.SortName,
                        UniqueId = SafeParser.Hash(artistByMusicBrainz.ApiKey.ToString())
                    });
                }
            }

            // Return first artist that matches and has album that matches any of the album names - the more matches ranks higher
            if (data.Count == 0 && query.AlbumKeyValues?.Length > 0)
            {
                var artistsByNamedNormalizedWithMatchingAlbums = await scopedContext.Artists
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.NameNormalized,
                        x.AlternateNames,
                        x.ApiKey,
                        x.MusicBrainzId,
                        x.SortName,
                        x.RealName,
                        AlbumNames = x.Albums.Select(a => a.NameNormalized),
                        AlbumAlternateNames = x.Albums.Select(a => a.AlternateNames)
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
                                    (x.AlbumNames.Any(a => query.AlbumNamesNormalized.Contains(a)) ||
                                     x.AlbumAlternateNames.Any(a => query.AlbumNamesNormalized.Contains(a))))
                        .ToArray();

                    if (matchingWithAlbums.Length != 0)
                    {
                        data.AddRange(matchingWithAlbums.Select(x => new ArtistSearchResult
                        {
                            FromPlugin = DisplayName,
                            UniqueId = SafeParser.Hash(x.ApiKey.ToString()),
                            Rank = matchingWithAlbums.Length + 1,
                            Name = x.Name,
                            ApiKey = x.ApiKey,
                            SortName = x.SortName,
                            MusicBrainzId = x.MusicBrainzId
                        }));
                    }
                }
            }

            if (data.Count == 0)
            {
                var artistsByNamedNormalized = await scopedContext.Artists
                    .Select(x => new { x.Id, x.Name, x.NameNormalized, x.AlternateNames, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName })
                    .Where(x => x.NameNormalized == query.NameNormalized || (x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)))
                    .OrderBy(x => x.SortName)
                    .Take(maxResults)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (artistsByNamedNormalized.Length > 0)
                {
                    data.AddRange(artistsByNamedNormalized.Select(x => new ArtistSearchResult
                    {
                        FromPlugin = DisplayName,
                        UniqueId = SafeParser.Hash(x.ApiKey.ToString()),
                        Rank = 1,
                        Name = x.Name,
                        ApiKey = x.ApiKey,
                        SortName = x.SortName,
                        MusicBrainzId = x.MusicBrainzId
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

    public async Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(IHttpClientFactory httpClientFactory, int forArtist, int maxResults, CancellationToken cancellationToken = default)
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
