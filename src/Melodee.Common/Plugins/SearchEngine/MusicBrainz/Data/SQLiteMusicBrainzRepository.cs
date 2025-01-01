using System.Diagnostics;
using System.Globalization;
using Dapper;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SmartFormat;
using Album = Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Album;
using Artist = Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Artist;
using Directory = System.IO.Directory;


namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;

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
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    public override async Task<Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
        {
            return db.Single<Album>(x => x.MusicBrainzId == musicBrainzId);
        }
    }

    public override async Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        var maxLuceneResults = 10;
        long totalCount = 0;

        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath) ?? throw new InvalidOperationException($"Invalid setting for [{SettingRegistry.SearchEngineMusicBrainzStoragePath}]");

        var musicBrainzIdsFromLucene = new List<string>();

        if (query.MusicBrainzIdValue != null)
        {
            musicBrainzIdsFromLucene.Add(query.MusicBrainzIdValue.Value.ToString());
        }
        else
        {
            using (var dir = FSDirectory.Open(Path.Combine(storagePath, "lucene")))
            {
                var analyzer = new StandardAnalyzer(AppLuceneVersion);
                var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
                using (var writer = new IndexWriter(dir, indexConfig))
                {
                    using (var reader = writer.GetReader(true))
                    {
                        var searcher = new IndexSearcher(reader);
                        BooleanQuery categoryQuery = [];
                        var catQuery1 = new TermQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalized));
                        var catQuery2 = new TermQuery(new Term(nameof(Artist.NameNormalized), query.NameNormalizedReversed));
                        var catQuery3 = new TermQuery(new Term(nameof(Artist.AlternateNames), query.NameNormalized));
                        categoryQuery.Add(new BooleanClause(catQuery1, Occur.SHOULD));
                        categoryQuery.Add(new BooleanClause(catQuery2, Occur.SHOULD));
                        categoryQuery.Add(new BooleanClause(catQuery3, Occur.SHOULD));
                        ScoreDoc[] hits = searcher.Search(categoryQuery, maxLuceneResults).ScoreDocs;
                        musicBrainzIdsFromLucene.AddRange(hits.Select(t => searcher.Doc(t.Doc)).Select(hitDoc => hitDoc.Get(nameof(Artist.MusicBrainzIdRaw))));
                    }
                }
            }
        }

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Name}] SearchArtist [{ArtistQuery}]", nameof(SQLiteMusicBrainzRepository), query))
            {
                using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
                {
                    var sql = """
                              SELECT Id, MusicBrainzArtistId, Name, SortName, NameNormalized, MusicBrainzIdRaw, AlternateNames
                              FROM "Artist" a
                              where a.MusicBrainzIdRaw in ('{0}')
                              order by a."SortName"
                              """;
                    var pSql = sql.FormatSmart(string.Join(@"','", musicBrainzIdsFromLucene));
                    var artists = db.Query<Artist>(pSql).ToArray();

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

                        if (artist.AlternateNamesValues.Contains(query.NameNormalizedReversed))
                        {
                            rank++;
                        }

                        var ssql = string.Empty;
                        if (query.AlbumMusicBrainzIds != null)
                        {
                            sql = """
                                  SELECT ReleaseType, ReleaseDate, MusicBrainzIdRaw, Name, NameNormalized, SortName, ReleaseGroupMusicBrainzIdRaw 
                                  FROM "Album"
                                  WHERE MusicBrainzIdRaw in ('{0}')
                                  group by ReleaseGroupMusicBrainzIdRaw
                                  order by ReleaseDate
                                  """;
                            ssql = sql.FormatSmart(string.Join(@"','", query.AlbumMusicBrainzIds));
                        }
                        else
                        {
                            sql = """
                                  SELECT ReleaseType, ReleaseDate, MusicBrainzIdRaw, Name, NameNormalized, SortName, ReleaseGroupMusicBrainzIdRaw 
                                  FROM "Album"
                                  WHERE MusicBrainzArtistId = {0}
                                  and ('{1}' = '' OR NameNormalized in ('{1}'))
                                  group by ReleaseGroupMusicBrainzIdRaw
                                  order by ReleaseDate
                                  """;
                            ssql = sql.FormatSmart(artist.MusicBrainzArtistId, string.Join(@"','", query.AlbumKeyValues?.Select(x => x.Value.ToNormalizedString()) ?? []), string.Join(@"','", query.AlbumKeyValues?.Select(x => x.Key) ?? []));
                        }

                        var artistAlbums = db.Query<Album>(ssql).ToArray();

                        rank += artistAlbums.Length;

                        if (query.AlbumKeyValues != null)
                        {
                            rank += artistAlbums.Length;
                            foreach (var albumKeyValues in query.AlbumKeyValues)
                            {
                                rank += artistAlbums.Count(x => x.ReleaseDate.Year.ToString() == albumKeyValues.Key && x.NameNormalized == albumKeyValues.Value.ToNormalizedString());
                            }
                        }

                        data.Add(new ArtistSearchResult
                        {
                            AlternateNames = artist.AlternateNames?.ToTags()?.ToArray() ?? [],
                            FromPlugin = $"{nameof(MusicBrainzArtistSearchEnginePlugin)}:{nameof(SQLiteMusicBrainzRepository)}",
                            UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                            Rank = rank,
                            Name = artist.Name,
                            SortName = artist.SortName,
                            MusicBrainzId = artist.MusicBrainzId,
                            AlbumCount = artistAlbums.Count(x => x is { DoIncludeInArtistSearch: true }),
                            Releases = artistAlbums
                                .Where(x => x is { DoIncludeInArtistSearch: true })
                                .OrderBy(x => x.ReleaseDate)
                                .ThenBy(x => x.SortName).Select(x => new AlbumSearchResult
                                {
                                    AlbumType = SafeParser.ToEnum<AlbumType>(x.ReleaseType),
                                    ReleaseDate = x.ReleaseDate.ToString("o", CultureInfo.InvariantCulture),
                                    UniqueId = SafeParser.Hash(x.MusicBrainzId.ToString()),
                                    Name = x.Name,
                                    NameNormalized = x.NameNormalized,
                                    MusicBrainzResourceGroupId = x.ReleaseGroupMusicBrainzId,
                                    SortName = x.SortName,
                                    MusicBrainzId = x.MusicBrainzId
                                }).ToArray()
                        });
                    }

                    totalCount = artists.Length;
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
            var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

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
            using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Created Lucene Index"))
            {
                var lucenePath = Path.Combine(storagePath, "lucene");
                if (Directory.Exists(lucenePath))
                {
                    Directory.Delete(lucenePath, true);
                }

                using (var dir = FSDirectory.Open(lucenePath))
                {
                    var analyzer = new StandardAnalyzer(AppLuceneVersion);
                    var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
                    using (var writer = new IndexWriter(dir, indexConfig))
                    {
                        foreach (var artist in LoadedMaterializedArtists)
                        {
                            var artistDoc = new Document
                            {
                                new StringField(nameof(Artist.MusicBrainzIdRaw),
                                    artist.MusicBrainzIdRaw,
                                    Field.Store.YES),
                                new StringField(nameof(Artist.NameNormalized),
                                    artist.NameNormalized,
                                    Field.Store.YES),
                                new TextField(nameof(Artist.AlternateNames),
                                    artist.AlternateNames ?? string.Empty,
                                    Field.Store.YES)
                            };
                            writer.AddDocument(artistDoc);
                        }

                        writer.Flush(false, false);
                    }
                }
            }

            using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
            {
                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded artists"))
                {
                    db.CreateTable<Artist>();
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

                    Log.Debug("MusicBrainzRepository: Imported [{Count}] artists of [{Loaded}] in [{BatchCount}] batches.", db.Count<Artist>(), LoadedMaterializedArtists.Count, batches);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded artist relations"))
                {
                    db.CreateTable<ArtistRelation>();

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

                    Log.Debug("MusicBrainzRepository: Imported [{Count}] artist relations of [{Loaded}] in [{BatchCount}] batches.", db.Count<ArtistRelation>(), LoadedMaterializedArtistRelations.Count, batches);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Inserted loaded albums"))
                {
                    db.CreateTable<Album>();
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

                    Log.Debug("MusicBrainzRepository: Imported [{Count}] albums of [{Loaded}] in [{BatchCount}] batches.", db.Count<Album>(), LoadedMaterializedAlbums.Count, batches);
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
