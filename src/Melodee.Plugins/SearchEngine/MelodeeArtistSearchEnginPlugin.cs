using System.Diagnostics;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Plugins.SearchEngine;

public class MelodeeArtistSearchEnginPlugin(IDbContextFactory<MelodeeDbContext> contextFactory) : IArtistSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "018A798D-7B68-4F3E-80CD-1BAF03998C0B";

    public string DisplayName => "Melodee Database";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 1;

    public async Task<PagedResult<ArtistSearchResult>> DoSearchAsync(IHttpClientFactory httpClientFactory, ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var startTicks = Stopwatch.GetTimestamp();
            var data = new List<ArtistSearchResult>();

            // var dbConn = scopedContext.Database.GetDbConnection();
            // return await dbConn
            //     .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Libraries\" WHERE \"ApiKey\" = @apiKey", new { apiKey })
            //     .ConfigureAwait(false);

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
                        FromPlugin = DisplayName,
                        UniqueId = SafeParser.Hash(artistByMusicBrainz.ApiKey.ToString()),
                        Rank = byte.MaxValue,
                        Name = artistByMusicBrainz.Name,
                        ApiKey = artistByMusicBrainz.ApiKey,
                        SortName = artistByMusicBrainz.SortName,
                        MusicBrainzId = artistByMusicBrainz.MusicBrainzId
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
}
