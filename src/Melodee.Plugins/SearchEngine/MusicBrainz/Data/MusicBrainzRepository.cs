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
using Serilog.Core;
using Serilog.Events;
using SerilogTimings;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Album = Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Album;
using Artist = Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Artist;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data;

using Artist = Artist;
using ArtistAlias = ArtistAlias;
using ArtistCredit = ArtistCredit;
using ArtistCreditName = ArtistCreditName;
using Release = Release;

/// <summary>
///     SQLite backend database created from MusicBrainz data dumps.
///     <remarks>
///         Worth noting the Service Stack ORM documentation says to 'always use synchronous APIs for SQLite';
///         https://docs.servicestack.net/ormlite/scalable-sqlite#always-use-synchronous-apis-for-sqlite
///         See https://metabrainz.org/datasets/postgres-dumps#musicbrainz
///     </remarks>
/// </summary>
public class MusicBrainzRepository(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    IDbConnectionFactory dbConnectionFactory)
{
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

    public async Task<Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default)
    {
        using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
        {
            return db.Single<Album>(x => x.MusicBrainzId == musicBrainzId);
        }
    }

    public async Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        int queryMax = 100;
        long totalCount = 0;

        try
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{Name}] SearchArtist [{ArtistQuery}]", nameof(MusicBrainzRepository), query))
            {
                using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
                {
                    if (query.MusicBrainzIdValue != null)
                    {
                        var artist = db.Single<Models.Materialized.Artist>(x => x.MusicBrainzId == query.MusicBrainzIdValue);
                        if (artist != null)
                        {
                            totalCount = 1;
                            List<Album> allArtistAlbums = db.Select<Album>(x => x.ArtistId == artist.Id) ?? [];

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
                                  SELECT Id, UniqueId, MusicBrainzArtistId, Name, SortName, NameNormalized, MusicBrainzIdRaw, AlternateNames
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
                                  SELECT Id, UniqueId, ArtistId, Name, SortName, NameNormalized, ReleaseType, ReleaseTypeValue, DoIncludeInArtistSearch, MusicBrainzIdRaw, ReleaseGroupMusicBrainzIdRaw, ReleaseDate, ContributorIds
                                  FROM "Album"
                                  WHERE ArtistId = @artistId
                                  LIMIT @queryMax;    
                                  """;
                            var allArtistAlbums = (await db.QueryAsync<Album>(sql, new { queryMax, artistId = artist.Id }).ConfigureAwait(false)).ToArray();

                            var artistAlbums = allArtistAlbums.GroupBy(x => x.NameNormalized).Select(x => x.OrderBy(xx => xx.ReleaseDate).FirstOrDefault()).ToArray();

                            if (query.AlbumKeyValues != null)
                            {
                                artistAlbums = artistAlbums.Where(x => query.AlbumKeyValues.Any(xx => x != null && xx.Key == x.ReleaseDate.Year.ToString() ||
                                                                                                      x != null && x.NameNormalized.Equals(xx.Value ?? string.Empty) ||
                                                                                                      x != null && x.NameNormalized.Contains(xx.Value ?? string.Empty))).ToArray();
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
            logger.Error(e, "[MusicBrainzRepository] Search Engine Exception AritistQuery [{Query}]", query.ToString());
        }

        return new PagedResult<ArtistSearchResult>
        {
            OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
            TotalCount = totalCount,
            TotalPages = SafeParser.ToNumber<int>((totalCount + maxResults - 1) / maxResults),
            Data = data.OrderByDescending(x => x.Rank).Take(maxResults).ToArray()
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

                LinkType[] linkTypes;
                Link[] links;
                LinkArtistToArtist[] linkArtistToArtists;

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


                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded link_type"))
                {
                    linkTypes = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/link_type"), parts => new LinkType
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        ParentId = SafeParser.ToNumber<long>(parts[1]),
                        ChildOrder = SafeParser.ToNumber<int>(parts[2]),
                        MusicBrainzId = SafeParser.ToGuid(parts[3]) ?? Guid.Empty,
                        EntityType0 = parts[4],
                        EntityType1 = parts[5],
                        Name = parts[6],
                        Description = parts[7],
                        LinkPhrase = parts[8],
                        ReverseLinkPhrase = parts[9],
                        HasDates = SafeParser.ToBoolean(parts[13]),
                        Entity0Cardinality = SafeParser.ToNumber<int>(parts[14]),
                        Entity1Cardinality = SafeParser.ToNumber<int>(parts[15])
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded link"))
                {
                    links = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/link"), parts => new Link
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        LinkTypeId = SafeParser.ToNumber<long>(parts[1]),
                        BeginDateYear = SafeParser.ToNumber<int?>(parts[2]),
                        BeginDateMonth = SafeParser.ToNumber<int?>(parts[3]),
                        BeginDateDay = SafeParser.ToNumber<int?>(parts[4]),
                        EndDateYear = SafeParser.ToNumber<int?>(parts[5]),
                        EndDateMonth = SafeParser.ToNumber<int?>(parts[6]),
                        EndDateDay = SafeParser.ToNumber<int?>(parts[7]),
                        IsEnded = SafeParser.ToBoolean(parts[10])
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist link"))
                {
                    linkArtistToArtists = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/l_artist_artist"), parts => new LinkArtistToArtist
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        LinkId = SafeParser.ToNumber<long>(parts[1]),
                        Artist0 = SafeParser.ToNumber<long>(parts[2]),
                        Artist1 = SafeParser.ToNumber<long>(parts[3]),
                        LinkOrder = SafeParser.ToNumber<int>(parts[6]),
                        Artist0Credit = parts[7],
                        Artist1Credit = parts[8]
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
                        NameNormalized = artist.NameNormalized,
                        MusicBrainzIdRaw = artist.MusicBrainzId.ToString(),
                        AlternateNames = "".AddTag(aArtistAlias?.Select(x => x.Name.ToNormalizedString() ?? x.Name), dontLowerCase: true)
                    });
                    if (artistsToInsert.Count >= batchSize || artist == artists.Last())
                    {
                        db.InsertAll(artistsToInsert);
                        artistCountInserted += artistsToInsert.Count;
                        artistsToInsert.Clear();
                        logger.Debug("MusicBrainzRepository: ImportData: inserted [{NumberInserted}] of [{NumberToInsert}]", artistCountInserted, artists.Length);
                    }

                    if (artistCountInserted > maxToProcess)
                    {
                        break;
                    }
                }

                // A dictionary of Artists in Melodee database using the MusicBrainz database Artist.Id as the key.
                Dictionary<long, Models.Materialized.Artist> dbArtistDictionary;
                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist from database"))
                {
                    dbArtistDictionary = db.Select<Models.Materialized.Artist>(x => x.Id > 0)
                        .ToDictionary(x => x.MusicBrainzArtistId, x => x);
                }

                var artistLinks = linkArtistToArtists.GroupBy(x => x.Artist0).ToDictionary(x => x.Key, x => x.ToArray());
                var associatedArtistRelationType = SafeParser.ToNumber<int>(ArtistRelationType.Associated);
                var artistRelationsToInsert = new List<ArtistRelation>();
                foreach (var artistLink in artistLinks)
                {
                    if (dbArtistDictionary.TryGetValue(artistLink.Key, out var dbArtist))
                    {
                        foreach (var artistLinkRelation in artistLink.Value)
                        {
                            if (dbArtistDictionary.TryGetValue(artistLinkRelation.Artist1, out var dbLinkedArtist))
                            {
                                var link = links.FirstOrDefault(x => x.Id == artistLink.Key);
                                var linkType = linkTypes.FirstOrDefault(x => x.Id == artistLinkRelation.LinkId);
                                if (linkType != null && link != null)
                                {
                                    artistRelationsToInsert.Add(new ArtistRelation
                                    {
                                        ArtistId = dbArtist.Id,
                                        RelatedArtistId = dbLinkedArtist.Id,
                                        ArtistRelationType = associatedArtistRelationType,
                                        SortOrder = artistLinkRelation.LinkOrder,
                                        RelationStart = link.BeginDate,
                                        RelationEnd = link.EndDate
                                    });
                                }
                            }
                        }
                    }
                }

                if (artistRelationsToInsert.Count > 0)
                {
                    db.InsertAll(artistRelationsToInsert);
                }

                Release[] releases;
                ReleaseCountry[] releasesCountries;
                Tag[] tags;
                ReleaseTag[] releaseTags;
                ReleaseGroup[] releaseGroups;
                ReleaseGroupMeta[] releaseGroupMetas;


                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release"))
                {
                    releases = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release"), parts => new Release
                    {
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        MusicBrainzId = parts[1],
                        Name = parts[2],
                        NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                        SortName = parts[2].CleanString(true),
                        ReleaseGroupId = SafeParser.ToNumber<long>(parts[4])
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
                        MusicBrainzIdRaw = parts[1],
                        Name = parts[2],
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                        ReleaseType = SafeParser.ToNumber<int>(parts[4])
                    }, cancellationToken).ConfigureAwait(false);
                }

                using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_group_meta"))
                {
                    releaseGroupMetas = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_group_meta"), parts => new ReleaseGroupMeta
                    {
                        ReleaseGroupId = SafeParser.ToNumber<long>(parts[0]),
                        DateYear = SafeParser.ToNumber<int>(parts[2]),
                        DateMonth = SafeParser.ToNumber<int>(parts[3]),
                        DateDay = SafeParser.ToNumber<int>(parts[4])
                    }, cancellationToken).ConfigureAwait(false);
                }

                db.CreateTable<Album>();
                var albumsToInsert = new List<Album>();

                var releaseCountriesDictionary = releasesCountries.GroupBy(x => x.ReleaseId).ToDictionary(x => x.Key, x => x.ToList());
                var releaseGroupsDictionary = releaseGroups.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());
                var releaseGroupsMetaDictionary = releaseGroupMetas.GroupBy(x => x.ReleaseGroupId).ToDictionary(x => x.Key, x => x.ToList());
                var artistCreditsDictionary = artistCredits.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());
                var artistCreditsNamesDictionary = artistCreditNames.GroupBy(x => x.ArtistCreditId).ToDictionary(x => x.Key, x => x.ToList());

                string? contributorIds = null;
                var invalidCount = 0;
                logger.Debug("MusicBrainzRepository: Loaded ReleaseCountries [{RcCount}] ReleaseGroups [{RgCount}] ReleaseGroupMetas [{RgMetaCount}] Artists [{ACount}]",
                    releaseCountriesDictionary.Count, releaseGroupsDictionary.Count, releaseGroupsMetaDictionary.Count, dbArtistDictionary.Count);
                foreach (var release in releases)
                {
                    releaseCountriesDictionary.TryGetValue(release.Id, out var releaseCountries);
                    releaseGroupsDictionary.TryGetValue(release.ReleaseGroupId, out var releaseReleaseGroups);
                    var releaseCountry = releaseCountries?.OrderBy(x => x.ReleaseDate).FirstOrDefault();
                    var releaseGroup = releaseReleaseGroups?.FirstOrDefault();
                    Models.Materialized.Artist? releaseArtist;
                    dbArtistDictionary.TryGetValue(release.ArtistCreditId, out releaseArtist);

                    if (releaseGroup != null && !(releaseCountry?.IsValid ?? false))
                    {
                        releaseGroupsMetaDictionary.TryGetValue(release.ReleaseGroupId, out var releaseGroupsMeta);
                        var releaseGroupMeta = releaseGroupsMeta?.OrderBy(x => x.ReleaseDate).FirstOrDefault();
                        if (releaseGroupMeta?.IsValid ?? false)
                        {
                            releaseCountry = new ReleaseCountry
                            {
                                ReleaseId = release.Id,
                                DateDay = releaseGroupMeta.DateDay,
                                DateMonth = releaseGroupMeta.DateMonth,
                                DateYear = releaseGroupMeta.DateYear
                            };
                        }
                    }

                    artistCreditsDictionary.TryGetValue(release.ArtistCreditId, out var releaseArtistCredits);
                    var artistCredit = releaseArtistCredits?.FirstOrDefault();
                    if (artistCredit != null)
                    {
                        artistCreditsNamesDictionary.TryGetValue(artistCredit.Id, out var releaseArtistCreditNames);
                        var artistCreditName = releaseArtistCreditNames?.OrderBy(x => x.Position).FirstOrDefault();
                        if (artistCreditName != null)
                        {
                            // Sometimes there are multiple artists on a release (see https://musicbrainz.org/release/519345af-b328-4d88-98cb-29f1a5d1fe2d) and the ArtistCreditId doesn't point to an ArtistId it points to a ArtistCredit.Id
                            //   when this is true then get ArtistCredit for that Id then get the ArtistCreditNames and the first one is used for the artist for Melodee
                            if (releaseArtist == null)
                            {
                                dbArtistDictionary.TryGetValue(artistCreditName.ArtistId, out releaseArtist);
                            }
                        }

                        var artistCreditNameArtistId = artistCreditName?.ArtistId ?? 0;
                        contributorIds = releaseArtistCreditNames == null
                            ? null
                            : "".AddTag(releaseArtistCreditNames
                                .Where(x => x.ArtistId != artistCreditNameArtistId)
                                .Select(x => x.ArtistId.ToString()));
                    }

                    if (releaseArtist != null && releaseGroup != null && (releaseCountry?.IsValid ?? false))
                    {
                        albumsToInsert.Add(new Album
                        {
                            ArtistId = releaseArtist.Id,
                            ContributorIds = contributorIds,
                            MusicBrainzIdRaw = release.MusicBrainzId,
                            Name = release.Name,
                            NameNormalized = release.NameNormalized ?? release.Name,
                            ReleaseDate = releaseCountry.ReleaseDate,
                            ReleaseGroupMusicBrainzIdRaw = releaseGroup.MusicBrainzIdRaw,
                            ReleaseType = releaseGroup.ReleaseType,
                            SortName = release.SortName ?? release.Name,
                            UniqueId = SafeParser.Hash(release.MusicBrainzId)
                        });
                    }
                    else
                    {
                        logger.Warning("Unable to find required data for Release [{Release}]: Artist [{ArtistCheck}] ReleaseGroup [{RgCheck}] ReleaseCountry [{RcCheck}]",
                            release, releaseArtist != null, releaseGroup != null, releaseCountry != null);
                        invalidCount++;
                    }

                    if (albumsToInsert.Count >= batchSize || release == releases.Last())
                    {
                        db.InsertAll(albumsToInsert);
                        releaseCountInserted += albumsToInsert.Count;
                        albumsToInsert.Clear();
                        logger.Debug("MusicBrainzRepository: ImportData: invalid found [{InvalidCount}], inserted [{NumberInserted}] of [{NumberToInsert}]",
                            invalidCount, releaseCountInserted, releases.Length);
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
