using System.Diagnostics;
using System.Globalization;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SmartFormat;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data;

/// <summary>
///     SQLite backend database created from MusicBrainz data dumps.
///     <remarks>
///         Worth noting the Service Stack ORM documentation says to 'always use synchronous APIs for SQLite';
///         https://docs.servicestack.net/ormlite/scalable-sqlite#always-use-synchronous-apis-for-sqlite
///         See https://metabrainz.org/datasets/postgres-dumps#musicbrainz
///     </remarks>
/// </summary>
public class SQLiteMusicBrainzRepository(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbConnectionFactory dbConnectionFactory) : MusicBrainzRepositoryBase(logger, configurationFactory)
{
    public override async Task<Models.Materialized.Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
        {
            return db.Single<Models.Materialized.Album>(x => x.MusicBrainzId == musicBrainzId);
        }
    }

    public override async Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        int queryMax = 100;
        long totalCount = 0;

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Name}] SearchArtist [{ArtistQuery}]", nameof(SQLiteMusicBrainzRepository), query))
            {
                using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
                {
                    if (query.MusicBrainzIdValue != null)
                    {
                        var artist = db.Single<Models.Materialized.Artist>(x => x.MusicBrainzId == query.MusicBrainzIdValue);
                        if (artist != null)
                        {
                            totalCount = 1;
                            List<Models.Materialized.Album> allArtistAlbums = db.Select<Models.Materialized.Album>(x => x.MusicBrainzArtistId == artist.MusicBrainzArtistId) ?? [];

                            var artistAlbums = allArtistAlbums.GroupBy(x => x.NameNormalized).Select(x => x.OrderBy(xx => xx.ReleaseDate).FirstOrDefault()).ToArray();

                            if (query.AlbumKeyValues != null)
                            {
                                artistAlbums = artistAlbums
                                    .Where(x => query.AlbumKeyValues.Any(xx => xx.Key == x?.ReleaseDate.Year.ToString() ||
                                                                               x != null && 
                                                                               x.NameNormalized.Contains(xx.Value ?? string.Empty)))
                                    .ToArray();
                            }

                            data.Add(new ArtistSearchResult
                            {
                                AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                                FromPlugin = nameof(MusicBrainzArtistSearchEnginPlugin),
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = short.MaxValue,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x is { DoIncludeInArtistSearch: true}),
                                Releases = artistAlbums.Where(x => x is { DoIncludeInArtistSearch: true }).OrderBy(x => x!.ReleaseDate).ThenBy(x => x!.SortName).Select(x => new AlbumSearchResult
                                {
                                    AlbumType = SafeParser.ToEnum<AlbumType>(x!.ReleaseType),
                                    ReleaseDate = x.ReleaseDate.ToString("o", CultureInfo.InvariantCulture),
                                    UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                                    Name = x.Name,
                                    NameNormalized = x.NameNormalized,
                                    SortName = x.SortName,
                                    MusicBrainzId = x.MusicBrainzId
                                }).ToArray()
                            });
                        }
                    }

                    if (data.Count == 0)
                    {
                        var sql = """
                                  SELECT Id, MusicBrainzArtistId, Name, SortName, NameNormalized, MusicBrainzIdRaw, AlternateNames
                                  FROM "Artist" a
                                  where a.NameNormalized LIKE @name
                                  OR a.AlternateNames LIKE @name
                                  order by a."SortName"
                                  LIMIT @queryMax
                                  """;
                        var artists = (await db.QueryAsync<Models.Materialized.Artist>(sql, new { queryMax, name = query.QueryNameNormalizedValue }).ConfigureAwait(false)).ToArray();

                        foreach (var artist in artists)
                        {
                            var rank = artist.NameNormalized == query.NameNormalized ? 10 : 1;
                            if (artist.AlternateNamesValues.Contains(query.NameNormalized))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.Name.CleanString().ToNormalizedString()))
                            {
                                rank++;
                            }

                            if (artist.AlternateNamesValues.Contains(query.NameReversed))
                            {
                                rank++;
                            }
                            sql = """
                                  SELECT ReleaseType, ReleaseDate, MusicBrainzIdRaw, Name, NameNormalized, SortName, ReleaseGroupMusicBrainzIdRaw 
                                  FROM "Album"
                                  WHERE MusicBrainzArtistId = {0}
                                  and ('{1}' = '' OR NameNormalized in ('{1}'))
                                  and ('{2}' = '' OR SUBSTR(ReleaseDate, 0, 5) in ('{2}'))
                                  group by ReleaseGroupMusicBrainzIdRaw
                                  order by ReleaseDate
                                  """;
                            var ssql = sql.FormatSmart(artist.MusicBrainzArtistId, string.Join(",", query.AlbumKeyValues?.Select(x => x.Value) ?? []), string.Join(",", query.AlbumKeyValues?.Select(x => x.Key) ?? []));
                            var artistAlbums = (await db.QueryAsync<Models.Materialized.Album>(ssql).ConfigureAwait(false)).ToArray();
                            rank += artistAlbums.Length;                            
                            
                            if (query.AlbumKeyValues != null)
                            {
                                artistAlbums = artistAlbums.Where(x => query.AlbumKeyValues.Any(xx => x != null && xx.Key == x.ReleaseDate.Year.ToString() &&
                                                                                                      (x != null && x.NameNormalized.Equals(xx.Value ?? string.Empty) ||
                                                                                                       x != null && x.NameNormalized.Contains(xx.Value ?? string.Empty)))).ToArray();
                                rank += artistAlbums.Length;
                            }

                            data.Add(new ArtistSearchResult
                            {
                                AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                                FromPlugin = $"{ nameof(MusicBrainzArtistSearchEnginPlugin)}:{ nameof(SQLiteMusicBrainzRepository)}",
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = rank,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x is { DoIncludeInArtistSearch: true}),
                                Releases = artistAlbums
                                    .Where(x => x is { DoIncludeInArtistSearch: true})
                                    .OrderBy(x => x!.ReleaseDate)
                                    .ThenBy(x => x!.SortName).Select(x => new AlbumSearchResult
                                    {
                                        AlbumType = SafeParser.ToEnum<AlbumType>(x!.ReleaseType),
                                        ReleaseDate = x.ReleaseDate.ToString("o", CultureInfo.InvariantCulture),
                                        UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                                        Name = x.Name,
                                        NameNormalized = x.NameNormalized,
                                        SortName = x.SortName,
                                        MusicBrainzId = x.MusicBrainzId
                                    }).ToArray()
                            });
                        }

                        totalCount = artists.Length;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "[MusicBrainzRepository] Search Engine Exception AritistQuery [{Query}]", query.ToString());
        }

        return new PagedResult<ArtistSearchResult>
        {
            OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
            TotalCount = totalCount,
            TotalPages = SafeParser.ToNumber<int>((totalCount + maxResults - 1) / maxResults),
            Data = data.OrderByDescending(x => x.Rank).Take(maxResults).ToArray()
        };
    }

    public override async Task<OperationResult<bool>> ImportData(CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: ImportData"))
        {
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            var batchSize = configuration.GetValue<int>(SettingRegistry.SearchEngineMusicBrainzImportBatchSize);
            var maxToProcess = configuration.GetValue<int>(SettingRegistry.SearchEngineMusicBrainzImportMaximumToProcess);
            if (maxToProcess == 0)
            {
                maxToProcess = int.MaxValue;
            }

            var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
            if (storagePath == null || !Directory.Exists(storagePath))
            {
                Logger.Warning("MusicBrainz storage path is invalid [{KeyNam}]", SettingRegistry.SearchEngineMusicBrainzStoragePath);
                return new OperationResult<bool>
                {
                    Data = false
                };
            }
            
            await LoadDataFromMusicBrainzFiles(cancellationToken).ConfigureAwait(false);
            
            using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
            {
                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded artists"))
                {
                    db.CreateTable<Models.Materialized.Artist>();
                    var batches = (LoadedMaterializedArtists.Count + batchSize - 1) / batchSize;
                    Log.Debug("MusicBrainzRepository: Importing [{BatchCount}] Artist batches...", batches);
                    for (var batch = 0; batch < batches; batch++)
                    {
                        db.InsertAll(LoadedMaterializedArtists.Skip(batch * batchSize).Take(batchSize));
                        if (batch * batchSize > maxToProcess)
                        {
                            break;
                        }                         
                    }
                    Log.Debug("MusicBrainzRepository: Imported [{Count}] artists of [{Loaded}] in [{BatchCount}] batches.", await db.CountAsync<Models.Materialized.Artist>(token: cancellationToken), LoadedMaterializedArtists.Count, batches);                    
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded artist relations"))
                {
                    db.CreateTable<Models.Materialized.ArtistRelation>();

                    var batches = (LoadedMaterializedArtistRelations.Count + batchSize - 1) / batchSize;
                    Log.Debug("MusicBrainzRepository: Importing [{BatchCount}] Artist Relations batches...", batches);
                    for (var batch = 0; batch < batches; batch++)
                    {
                        db.InsertAll(LoadedMaterializedArtistRelations.Skip(batch * batchSize).Take(batchSize));
                        if (batch * batchSize > maxToProcess)
                        {
                            break;
                        }
                    }
                    Log.Debug("MusicBrainzRepository: Imported [{Count}] artist relations of [{Loaded}] in [{BatchCount}] batches.", await db.CountAsync<Models.Materialized.ArtistRelation>(token: cancellationToken), LoadedMaterializedArtistRelations.Count, batches);                    
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded albums"))
                {
                    db.CreateTable<Models.Materialized.Album>();
                    var batches = (LoadedMaterializedAlbums.Count + batchSize - 1) / batchSize;
                    Log.Debug("MusicBrainzRepository: Importing [{BatchCount}] Album batches...", batches);
                    for (var batch = 0; batch < batches; batch++)
                    {
                        db.InsertAll(LoadedMaterializedAlbums.Skip(batch * batchSize).Take(batchSize));
                        if (batch * batchSize > maxToProcess)
                        {
                            break;
                        }  
                    }
                    Log.Debug("MusicBrainzRepository: Imported [{Count}] albums of [{Loaded}] in [{BatchCount}] batches.", await db.CountAsync<Models.Materialized.Album>(token: cancellationToken), LoadedMaterializedAlbums.Count, batches);                    
                }

            }

            return new OperationResult<bool>
            {
                Data = LoadedMaterializedArtists.Count > 0 &&
                       LoadedMaterializedAlbums.Count > 0
            };
        }
    }
}
