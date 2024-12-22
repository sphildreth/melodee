using System.Diagnostics;
using System.Globalization;
using LiteDB;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data;

/// <summary>
///     LiteDB backend database created from MusicBrainz data dumps.
///     <remarks>
///         sph; I put this together thinking it would be faster than SQLite, I was wrong. Leaving here to remind me
///              that it is not faster than SQLite.  
///     </remarks>
/// </summary>
public sealed class LiteDbMusicBrainzRepository(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory) : MusicBrainzRepositoryBase(logger, configurationFactory)
{
    private const string DbName = "litedb-musicbrainz.db";

    public override async Task<Models.Materialized.Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        var storagePath = await StoragePath(cancellationToken).ConfigureAwait(false);
        using (var db = new LiteDatabase(@$"{storagePath}{DbName}"))
        {
            var col = db.GetCollection<Models.Materialized.Album>(Models.Materialized.Album.TableName);
            return col.FindOne(x => x.MusicBrainzId == musicBrainzId);
        }
    }

    public override async Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        long totalCount = 0;

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Name}] SearchArtist [{ArtistQuery}]", nameof(LiteDbMusicBrainzRepository), query))
            {
                var storagePath = await StoragePath(cancellationToken).ConfigureAwait(false);
                using (var db = new LiteDatabase(@$"{storagePath}{DbName}"))
                {
                    var artistCol = db.GetCollection<Models.Materialized.Artist>(Models.Materialized.Artist.TableName);
                    var albumCol = db.GetCollection<Models.Materialized.Album>(Models.Materialized.Album.TableName);

                    if (query.MusicBrainzIdValue != null)
                    {
                        var artist = artistCol.FindOne(x => x.MusicBrainzId == query.MusicBrainzIdValue);
                        if (artist != null)
                        {
                            totalCount = 1;
                            List<Models.Materialized.Album> allArtistAlbums = albumCol.Find(x => x.MusicBrainzArtistId == artist.MusicBrainzArtistId).ToList();

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
                                FromPlugin = $"{ nameof(MusicBrainzArtistSearchEnginPlugin)}:{ nameof(LiteDbMusicBrainzRepository)}",
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = short.MaxValue,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x is { DoIncludeInArtistSearch: true }),
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
                        var artists = artistCol.Find(x => x.NameNormalized.Contains(query.NameNormalized) || x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)).ToArray();

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
                            var artistAlbums = albumCol.Find(x => x.MusicBrainzArtistId == artist.Id).ToArray();
                            rank += artistAlbums.Length;

                            if (query.AlbumKeyValues != null)
                            {
                                artistAlbums = artistAlbums.Where(x => query.AlbumKeyValues.Any(xx => x != null && xx.Key == x.ReleaseDate.Year.ToString() &&
                                                                                                      (x.NameNormalized.Equals(xx.Value ?? string.Empty) ||
                                                                                                       x.NameNormalized.Contains(xx.Value ?? string.Empty)))).ToArray();
                                rank += artistAlbums.Length;
                            }

                            data.Add(new ArtistSearchResult
                            {
                                AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                                FromPlugin = nameof(MusicBrainzArtistSearchEnginPlugin),
                                UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                                Rank = rank,
                                Name = artist.Name,
                                SortName = artist.SortName,
                                MusicBrainzId = artist.MusicBrainzId,
                                AlbumCount = artistAlbums.Count(x => x is { DoIncludeInArtistSearch: true }),
                                Releases = artistAlbums
                                    .Where(x => x is { DoIncludeInArtistSearch: true })
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

                            totalCount = artists.Length;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "[MusicBrainzRepository] Search Engine Exception ArtistQuery [{Query}]", query.ToString());
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
        var storagePath = await StoragePath(cancellationToken).ConfigureAwait(false);

        await LoadDataFromMusicBrainzFiles(cancellationToken).ConfigureAwait(false);

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var batchSize = configuration.GetValue<int>(SettingRegistry.SearchEngineMusicBrainzImportBatchSize);

        using (var db = new LiteDatabase(@$"{storagePath}{DbName}"))
        {
            using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded artists"))
            {
                var col = db.GetCollection<Models.Materialized.Artist>(Models.Materialized.Artist.TableName);

                col.EnsureIndex(x => x.MusicBrainzId, true);
                col.EnsureIndex(x => x.Name);
                col.EnsureIndex(x => x.NameNormalized);

                var batches = (LoadedMaterializedArtists.Count + batchSize - 1) / batchSize;
                Log.Debug("MusicBrainzRepository: Importing [{BatchCount}] Artist batches...", batches);
                for (var batch = 0; batch < batches; batch++)
                {
                    col.InsertBulk(LoadedMaterializedArtists.Skip(batch * batchSize).Take(batchSize));
                }

                Log.Debug("MusicBrainzRepository: Imported [{Count}] artists of [{Loaded}] in [{BatchCount}] batches.", col.Count(), LoadedMaterializedArtists.Count, batches);
            }

            using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded albums"))
            {
                var col = db.GetCollection<Models.Materialized.Album>(Models.Materialized.Album.TableName);

                col.EnsureIndex(x => x.MusicBrainzArtistId);
                col.EnsureIndex(x => x.MusicBrainzId, true);
                col.EnsureIndex(x => x.Name);
                col.EnsureIndex(x => x.NameNormalized);

                var batches = (LoadedMaterializedAlbums.Count + batchSize - 1) / batchSize;
                Log.Debug("MusicBrainzRepository: Importing [{BatchCount}] Album batches...", batches);
                for (var batch = 0; batch < batches; batch++)
                {
                    col.InsertBulk(LoadedMaterializedAlbums.Skip(batch * batchSize).Take(batchSize));
                }

                Log.Debug("MusicBrainzRepository: Imported [{Count}] albums of [{Loaded}] in [{BatchCount}] batches.", col.Count(), LoadedMaterializedAlbums.Count, batches);
            }
        }

        return new OperationResult<bool>
        {
            Data = LoadedMaterializedArtists.Count > 0 &&
                   LoadedMaterializedAlbums.Count > 0
        };
    }
}
