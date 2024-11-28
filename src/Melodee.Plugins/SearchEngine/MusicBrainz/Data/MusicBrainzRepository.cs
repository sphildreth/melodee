using System.Data;
using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Artist = Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Artist;
using StringExtensions = Melodee.Common.Extensions.StringExtensions;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data;

using Artist = Artist;
using ArtistAlias = ArtistAlias;
using ArtistCredit = ArtistCredit;
using ArtistCreditName = ArtistCreditName;
using Release = Release;

/// <summary>
///     SQLite backend database created from MusicBrainz data dumps.
///     <remarks>
///         See https://metabrainz.org/datasets/postgres-dumps#musicbrainz
///     </remarks>
/// </summary>
public class MusicBrainzRepository(
    ILogger logger,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    IDbConnectionFactory dbConnectionFactory)
{
    private AlbumSearchResult[] GetDataAlbums(AlbumSearchResult[] artistAlbums)
    {
        var artistReleases = new List<AlbumSearchResult>();
        foreach (var artistAlbum in artistAlbums)
        {
            if (artistReleases.Any(x => x.NameNormalized == artistAlbum.NameNormalized))
            {
                continue;
            }

            var releaseDatePart = artistAlbum.ReleaseDateParts?.Split(StringExtensions.TagsSeparator);
            string? releaseDate = null;
            if (releaseDatePart?.Length > 1)
            {
                releaseDate = $"{SafeParser.ToNumber<int>(releaseDatePart[0])}-{SafeParser.ToNumber<int>(releaseDatePart[1]).ToStringPadLeft(2)}-{SafeParser.ToNumber<int>(releaseDatePart[2]).ToStringPadLeft(2)}T00:00:00";
            }

            var musicBrainzId = SafeParser.ToGuid(artistAlbum.MusicBrainzIdRaw);

            artistReleases.Add(new AlbumSearchResult
            {
                ReleaseDate = releaseDate,
                UniqueId = SafeParser.Hash(musicBrainzId?.ToString() ?? artistAlbum.NameNormalized),
                Name = artistAlbum.Name,
                NameNormalized = artistAlbum.NameNormalized ?? artistAlbum.Name,
                SortName = artistAlbum.SortName ?? artistAlbum.Name,
                MusicBrainzId = musicBrainzId
            });
        }

        return artistReleases.OrderBy(x => x.ReleaseDate).ToArray();
    }

    private async Task<ArtistSearchResult> GetArtistSearchResultForArtistAsync(IDbConnection db, short rank, Artist artist, AlbumSearchResult[] artistAlbums, CancellationToken cancellationToken = default)
    {
        var artistAliasesQuery = db.From<ArtistAlias>().Where<ArtistAlias>(x => x.ArtistId == artist.Id);
        var artistAliases = (await db.SelectAsync<ArtistAlias>(artistAliasesQuery, cancellationToken)).ToArray() ?? [];
        return new ArtistSearchResult
        {
            AlternateNames = artistAliases
                .OrderBy(x => x.SortName)
                .Select(x => x.Name.ToNormalizedString() ?? x.Name)
                .Distinct()
                .ToArray(),
            FromPlugin = nameof(MusicBrainzArtistSearchEnginPlugin),
            UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
            Rank = rank,
            Name = artist.Name,
            SortName = artist.SortName,
            MusicBrainzId = artist.MusicBrainzId,
            AlbumCount = artistAlbums.Length,
            Releases = artistAlbums
        };
    }

    private static async Task<T[]> LoadDataFromFileAsync<T>(string file, Func<string[], T> constructor, CancellationToken cancellationToken = default) where T : notnull
    {
        if (!File.Exists(file))
        {
            return [];
        }

        var result = new List<T>();
        await foreach (var lineFromFile in File.ReadLinesAsync(file, cancellationToken))
        {
            var parts = lineFromFile.Split('\t');
            result.Add(constructor(parts));
        }

        return result.ToArray();
    }

    public async Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        long totalCount = 0;
        // using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
        // {
        //     var releaseSql = """
        //                      select r.Id, r.ArtistCreditId, r.Name, r.NameNormalized, r.SortName, r.MusicBrainzId as "MusicBrainzIdRaw",
        //                             (SELECT rc.DateYear || '|' || rc.DateMonth || '|' || rc.DateDay
        //                              FROM "ReleaseCountry" rc
        //                              where rc.ReleaseId = r.Id
        //                              order by rc.DateYear desc
        //                              LIMIT 1) as "ReleaseDateParts"
        //                      from "Release" r
        //                      where r.ArtistCreditId = @artistId
        //                      and "ReleaseDateParts" IS NOT NULL
        //                      order by Name, "ReleaseDateParts"
        //                      """;
        //     if (query.MusicBrainzId != null)
        //     {
        //         var artistByMusicBrainzId = db.Single<Artist>(x => x.MusicBrainzId == query.MusicBrainzIdValue);
        //         if (artistByMusicBrainzId != null)
        //         {
        //             totalCount = 1;
        //             data.Add(await GetArtistSearchResultForArtistAsync(
        //                     db,
        //                     short.MaxValue,
        //                     artistByMusicBrainzId,
        //                     GetDataAlbums((await db.QueryAsync<AlbumSearchResult>(releaseSql, new { artistId = artistByMusicBrainzId.Id }).ConfigureAwait(false)).ToArray()),
        //                     cancellationToken)
        //                 .ConfigureAwait(false));
        //         }
        //     }
        //
        //     if (data.Count == 0)
        //     {
        //         var sql = """
        //                   select DISTINCT COUNT(a."Id") as "TotalCount"
        //                   FROM "Artist" a
        //                   LEFT JOIN "ArtistAlias" aa on (a."Id" = aa."ArtistId")
        //                   where a."NameNormalized" like @normalizedName OR aa.Name like @name;
        //                   select DISTINCT  a.*
        //                   FROM "Artist" a
        //                   LEFT JOIN "ArtistAlias" aa on (a."Id" = aa."ArtistId")
        //                   where a."NameNormalized" like @normalizedName OR aa.Name like @name
        //                   LIMIT @maxResults;
        //                   """;
        //         using (var multi = await db.QueryMultipleAsync(sql, new { maxResults, normalizedName = $"%{query.NameNormalized}%", name = "%{query.Name}%" }))
        //         {
        //             totalCount = await multi.ReadFirstAsync<long>();
        //             var dynamicArtists = (await multi.ReadAsync()) ?? [];
        //             foreach (var dynamicArtist in dynamicArtists)
        //             {
        //                 var artist = new Artist
        //                 {
        //                     Id = dynamicArtist.Id,
        //                     MusicBrainzId = SafeParser.ToGuid(dynamicArtist.MusicBrainzId),
        //                     Name = dynamicArtist.Name,
        //                     NameNormalized = dynamicArtist.NameNormalized,
        //                     SortName = dynamicArtist.SortName
        //                 };
        //
        //                 var albumsForArtist = GetDataAlbums((await db.QueryAsync<AlbumSearchResult>(releaseSql, new { artistId = artist.Id }).ConfigureAwait(false) ?? []).ToArray());
        //                 short rank = 1;
        //                 var matchedAlbumSearchResults = new List<AlbumSearchResult>();
        //                 if (albumsForArtist.Length > 0 && query.AlbumKeyValues != null)
        //                 {
        //                     foreach (var keyValue in query.AlbumKeyValues)
        //                     {
        //                         var matchedAlbum = albumsForArtist.FirstOrDefault(x => x.KeyValue.Key == keyValue.Key || x.KeyValue.Value == keyValue.Value);
        //                         if (matchedAlbum != null)
        //                         {
        //                             matchedAlbumSearchResults.Add(matchedAlbum);
        //                             if (matchedAlbum.KeyValue.Key == keyValue.Key)
        //                             {
        //                                 // Add an extra rank if the Key matches
        //                                 rank++;
        //                             }
        //
        //                             rank++;
        //                         }
        //                     }
        //
        //                     albumsForArtist = matchedAlbumSearchResults.ToArray();
        //                 }
        //
        //                 data.Add(await GetArtistSearchResultForArtistAsync(db,
        //                         rank,
        //                         artist,
        //                         albumsForArtist,
        //                         cancellationToken)
        //                     .ConfigureAwait(false));
        //             }
        //         }
        //     }
        // }

        return new PagedResult<ArtistSearchResult>
        {
            OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
            TotalCount = totalCount,
            TotalPages = 0,
            Data = data
        };
    }

    public async Task<OperationResult<bool>> ImportData(CancellationToken cancellationToken = default)
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
                logger.Warning("MusicBrainz storage path is invalid [{KeyNam}]", SettingRegistry.SearchEngineMusicBrainzStoragePath);
                return new OperationResult<bool>
                {
                    Data = false
                };
            }

            var artistCountInserted = 0;
            var releaseCountInserted = 0;

            using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
            {
                Artist[] artists;
                ArtistCredit[] artistCredits;
                ArtistCreditName[] artistCreditNames;
                ArtistAlias[] artistAliases;
                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artists"))
                {
                    artists = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist"), parts => new Artist
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                        Name = parts[2],
                        NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                        SortName = parts[3].CleanString(true) ?? parts[2]
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_credit"))
                {
                    artistCredits = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit"), parts => new ArtistCredit
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        ArtistCount = SafeParser.ToNumber<int>(parts[2]),
                        Name = parts[1],
                        RefCount = SafeParser.ToNumber<int>(parts[3]),
                        Gid = SafeParser.ToGuid(parts[6]) ?? Guid.Empty
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_credit_name"))
                {
                    artistCreditNames = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit_name"), parts => new ArtistCreditName
                    {
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                        Position = SafeParser.ToNumber<int>(parts[1]),
                        ArtistId = SafeParser.ToNumber<long>(parts[2]),
                        Name = parts[3]
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_alias"))
                {
                    artistAliases = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_alias"), parts => new ArtistAlias
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        ArtistId = SafeParser.ToNumber<long>(parts[1]),
                        Name = parts[2],
                        Type = SafeParser.ToNumber<int>(parts[6]),
                        SortName = parts[7]
                    }, cancellationToken).ConfigureAwait(false);
                }

                db.CreateTable<Models.Materialized.Artist>();
                var artistsToInsert = new List<Models.Materialized.Artist>();
                var artistAliasDictionary = artistAliases.GroupBy(x => x.ArtistId).ToDictionary(x => x.Key, x => x.ToArray());
                foreach (var artist in artists)
                {
                    artistAliasDictionary.TryGetValue(artist.Id, out var aArtistAlias);
                    artistsToInsert.Add(new Models.Materialized.Artist
                    {
                        UniqueId = SafeParser.Hash(artist.MusicBrainzId.ToString()),
                        MusicBrainzArtistId = artist.Id,
                        Name = artist.Name,
                        SortName = artist.SortName,
                        NormalizedName = artist.NameNormalized,
                        MusicBrainzId = artist.MusicBrainzId,
                        AlternateNames = "".AddTag(aArtistAlias?.Select(x => x.Name.ToNormalizedString() ?? x.Name), dontLowerCase: true)
                    });
                    if (artistsToInsert.Count >= batchSize)
                    {
                        await db.InsertAllAsync(artistsToInsert, token: cancellationToken).ConfigureAwait(false);
                        artistCountInserted += artistsToInsert.Count;
                        artistsToInsert.Clear();
                        logger.Debug("MusicBrainzRepository: ImportData: inserted [{NumberInserted}] of [{NumberToInsert}]", artistCountInserted, artists.Length);
                    }

                    if (artistCountInserted > maxToProcess)
                    {
                        break;
                    }
                }

                Release[] releases;
                ReleaseCountry[] releasesCountries;
                Tag[] tags;
                ReleaseTag[] releaseTags;
                ReleaseGroup[] releaseGroups;

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release"))
                {
                    releases = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release"), parts => new Release
                    {
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                        Name = parts[2],
                        NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                        SortName = parts[2].CleanString(true)
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_country"))
                {
                    releasesCountries = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_country"), parts => new ReleaseCountry
                    {
                        ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                        CountryId = SafeParser.ToNumber<long>(parts[1]),
                        DateYear = SafeParser.ToNumber<int>(parts[2]),
                        DateMonth = SafeParser.ToNumber<int>(parts[3]),
                        DateDay = SafeParser.ToNumber<int>(parts[4])
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded tag"))
                {
                    tags = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/tag"), parts => new Tag
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        Name = parts[1]
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_tag"))
                {
                    releaseTags = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_tag"), parts => new ReleaseTag
                    {
                        ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                        TagId = SafeParser.ToNumber<long>(parts[1])
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_group"))
                {
                    releaseGroups = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_group"), parts => new ReleaseGroup
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        Name = parts[2],
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                        ReleaseType = SafeParser.ToNumber<int>(parts[4])
                    }, cancellationToken).ConfigureAwait(false);
                }

                db.CreateTable<Models.Materialized.Album>();
                var albumsToInsert = new List<Models.Materialized.Album>();

                foreach (var release in releases)
                {
                    var releaseCountry = releasesCountries
                        .Where(x => x.ReleaseId == release.Id)
                        .Select(x => x)
                        .OrderBy(x => x.ReleaseDate)
                        .FirstOrDefault();

                    //var artistCredit = artistCredits.FirstOrDefault(x => x.Id == release.ArtistCreditId);
                    var releaseGroup = releaseGroups.FirstOrDefault(x => x.Id == release.ReleaseGroupId);

                    var releaseArtist = (await db.SelectAsync<Models.Materialized.Artist>(x => x.MusicBrainzArtistId == release.ArtistCreditId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

                    if (releaseArtist != null && releaseGroup != null && releaseCountry != null)
                    {
                        albumsToInsert.Add(new Models.Materialized.Album
                        {
                            UniqueId = SafeParser.Hash(release.MusicBrainzId.ToString()),
                            ArtistId = releaseArtist.Id,
                            Name = release.Name,
                            SortName = release.SortName ?? release.Name,
                            ReleaseType = releaseGroup.ReleaseType,
                            NormalizedName = release.NameNormalized ?? release.Name,
                            MusicBrainzId = release.MusicBrainzId,
                            ReleaseDate = new DateOnly(releaseCountry.DateYear, releaseCountry.DateMonth, releaseCountry.DateDay)
                        });
                    }

                    if (albumsToInsert.Count >= batchSize)
                    {
                        await db.InsertAllAsync(albumsToInsert, token: cancellationToken).ConfigureAwait(false);
                        releaseCountInserted += albumsToInsert.Count;
                        albumsToInsert.Clear();
                        logger.Debug("MusicBrainzRepository: ImportData: inserted [{NumberInserted}] of [{NumberToInsert}]", releaseCountInserted, releases.Length);
                    }

                    if (releaseCountInserted > maxToProcess)
                    {
                        break;
                    }
                }
            }

            return new OperationResult<bool>
            {
                Data = artistCountInserted > 0 &&
                       releaseCountInserted > 0
            };
        }
    }
}
