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
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
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
///         See https://metabrainz.org/datasets/postgres-dumps#musicbrainz
///     </remarks>
/// </summary>
public class MusicBrainzRepository(
    ILogger logger,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    IDbConnectionFactory dbConnectionFactory)
{
    public async Task<OperationResult<ArtistSearchResult[]?>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var data = new List<ArtistSearchResult>();

        using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
        {
            //var meta = db.SqlList<CustomPoco>("SELECT DISTINCT Year % 10 as Year, hex(Year % 10) as Hex FROM Track");                

            if (query.MusicBrainzId != null)
            {
                var artistByMusicBrainzId = db.Single<Artist>(x => x.MusicBrainzId == query.MusicBrainzIdValue);
                if (artistByMusicBrainzId != null)
                {
                    // TODO Get Arist Aliases

                    var artistAlbumsQuery = db.From<Release>()
                        .Join<ArtistCreditName>((release, artistCreditName) => release.ArtistCreditId == artistCreditName.ArtistCreditId)
                        .Where<ArtistCreditName>(x => x.ArtistId == artistByMusicBrainzId.Id);
                    var artistAlbums = db.Select(artistAlbumsQuery) ?? [];
                    var artistReleases = new List<AlbumSearchResult>();
                    foreach (var artistAlbum in artistAlbums)
                    {
                        var uniqueId = SafeParser.Hash(artistAlbum.MusicBrainzId.ToString());
                        artistReleases.Add(new AlbumSearchResult
                        {
                            UniqueId = uniqueId,
                            Name = artistAlbum.Name,
                            NameNormalized = artistAlbum.NameNormalized ?? artistAlbum.Name,
                            SortName = artistAlbum.SortName ?? artistAlbum.Name,
                            MusicBrainzId = artistAlbum.MusicBrainzId
                        });
                    }

                    data.Add(new ArtistSearchResult
                    {
                        FromPlugin = nameof(MusicBrainzArtistSearchEnginPlugin),
                        UniqueId = SafeParser.Hash(artistByMusicBrainzId.MusicBrainzId.ToString()),
                        Rank = byte.MaxValue,
                        Name = artistByMusicBrainzId.Name,
                        SortName = artistByMusicBrainzId.SortName,
                        MusicBrainzId = artistByMusicBrainzId.MusicBrainzId,
                        Releases = artistReleases.ToArray()
                    });
                }
            }
            //
            // // Return first artist that matches and has album that matches any of the album names - the more matches ranks higher
            // if (data.Count == 0 && query.AlbumNames?.Length > 0)
            // {
            //     var artistsByNamedNormalizedWithMatchingAlbums = await scopedContext.Artists
            //         .Select(x => new
            //         {
            //             x.Id, 
            //             x.Name, 
            //             x.NameNormalized, 
            //             x.AlternateNames,
            //             x.ApiKey, 
            //             x.MusicBrainzId, 
            //             x.SortName, 
            //             x.RealName, 
            //             AlbumNames = x.Albums.Select(a => a.NameNormalized),
            //             AlbumAlternateNames = x.Albums.Select(a => a.AlternateNames)
            //         })
            //         .Where(x => x.NameNormalized == query.NameNormalized || (x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)) )
            //         .OrderBy(x => x.SortName)
            //         .Take(maxResults)
            //         .ToArrayAsync(cancellationToken)
            //         .ConfigureAwait(false);
            //     
            //     if (artistsByNamedNormalizedWithMatchingAlbums.Length > 0)
            //     {
            //         var matchingWithAlbums = artistsByNamedNormalizedWithMatchingAlbums
            //             .Where(x => query.AlbumNamesNormalized != null && 
            //                         (x.AlbumNames.Any(a => query.AlbumNamesNormalized.Contains(a)) || 
            //                          x.AlbumAlternateNames.Any(a => query.AlbumNamesNormalized.Contains(a))))
            //             .ToArray();
            //         
            //         if (matchingWithAlbums.Length != 0)
            //         {
            //             data.AddRange(matchingWithAlbums.Select(x => new ArtistSearchResult(
            //                 DisplayName,
            //                 SafeParser.Hash(x.ApiKey.ToString()),
            //                 matchingWithAlbums.Length+1,
            //                 x.Name,
            //                 ApiKey: x.ApiKey,
            //                 SortName: x.SortName,
            //                 MusicBrainzId: x.MusicBrainzId)));
            //         }
            //     }                
            // }
            //
            // if (data.Count == 0)
            // {
            //     var artistsByNamedNormalized = await scopedContext.Artists
            //         .Select(x => new { x.Id, x.Name, x.NameNormalized, x.AlternateNames, x.ApiKey, x.MusicBrainzId, x.SortName, x.RealName })
            //         .Where(x => x.NameNormalized == query.NameNormalized || (x.AlternateNames != null && x.AlternateNames.Contains(query.NameNormalized)) )
            //         .OrderBy(x => x.SortName)
            //         .Take(maxResults)
            //         .ToArrayAsync(cancellationToken)
            //         .ConfigureAwait(false);
            //
            //     if (artistsByNamedNormalized.Length > 0)
            //     {
            //         data.AddRange(artistsByNamedNormalized.Select(x => new ArtistSearchResult(
            //             DisplayName,
            //              SafeParser.Hash(x.ApiKey.ToString()),
            //             1,
            //              x.Name,
            //              ApiKey: x.ApiKey,
            //              SortName: x.SortName,
            //              MusicBrainzId: x.MusicBrainzId)));
            //     }
            // }
        }

        return new OperationResult<ArtistSearchResult[]?>
        {
            OperationTime = Stopwatch.GetElapsedTime(startTicks).Microseconds,
            Data = data.Count == 0 ? null : data.ToArray()
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
            var artistCreditInserted = 0;
            var artistCreditNameInserted = 0;
            var artistAliasesInserted = 0;
            var releaseCountInserted = 0;
            var releaseCountriesCountInserted = 0;
            var tagCountInserted = 0;
            var releaseTagsCountInserted = 0;

            using (var db = await dbConnectionFactory.OpenAsync(cancellationToken))
            {
                db.CreateTable<Artist>();
                var artistsToAdd = new List<Artist>();

                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/artist"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');
                    var artist = new Artist
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                        Name = parts[2],
                        NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                        SortName = parts[3].CleanString(true) ?? parts[2]
                    };
                    if (artist.IsValid)
                    {
                        artistsToAdd.Add(artist);
                        if (artistsToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(artistsToAdd, cancellationToken);
                            artistCountInserted += artistsToAdd.Count;
                            artistsToAdd.Clear();
                        }
                    }

                    if (artistCountInserted > maxToProcess)
                    {
                        break;
                    }
                }


                db.CreateTable<ArtistCredit>();
                var artistCreditsToAdd = new List<ArtistCredit>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var artistCredit = new ArtistCredit
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        ArtistCount = SafeParser.ToNumber<int>(parts[2]),
                        Name = parts[1],
                        RefCount = SafeParser.ToNumber<int>(parts[3]),
                        Gid = SafeParser.ToGuid(parts[6]) ?? Guid.Empty
                    };

                    if (artistCredit.IsValid)
                    {
                        artistCreditsToAdd.Add(artistCredit);
                        if (artistCreditsToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(artistCreditsToAdd, cancellationToken);
                            artistCreditInserted += artistCreditsToAdd.Count;
                            artistCreditsToAdd.Clear();
                        }
                    }

                    if (artistCreditInserted > maxToProcess)
                    {
                        break;
                    }
                }

                db.CreateTable<ArtistCreditName>();
                var artistCreditNamesToAdd = new List<ArtistCreditName>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit_name"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var artistCreditName = new ArtistCreditName
                    {
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                        Position = SafeParser.ToNumber<int>(parts[1]),
                        ArtistId = SafeParser.ToNumber<long>(parts[2]),
                        Name = parts[3]
                    };
                    if (artistCreditName.IsValid)
                    {
                        artistCreditNamesToAdd.Add(artistCreditName);
                        if (artistCreditNamesToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(artistCreditNamesToAdd, cancellationToken);
                            artistCreditNameInserted += artistCreditNamesToAdd.Count;
                            artistCreditNamesToAdd.Clear();
                        }
                    }

                    if (artistCreditNameInserted > maxToProcess)
                    {
                        break;
                    }
                }


                db.CreateTable<ArtistAlias>();
                var artistAliasToAdd = new List<ArtistAlias>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/artist_alias"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var artistAlias = new ArtistAlias
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        ArtistId = SafeParser.ToNumber<long>(parts[1]),
                        Name = parts[2],
                        Type = SafeParser.ToNumber<int>(parts[6]),
                        SortName = parts[7]
                    };

                    if (artistAlias.IsValid)
                    {
                        artistAliasToAdd.Add(artistAlias);
                        if (artistAliasToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(artistAliasToAdd, cancellationToken);
                            artistAliasesInserted += artistAliasToAdd.Count;
                            artistAliasToAdd.Clear();
                        }
                    }

                    if (artistAliasesInserted > maxToProcess)
                    {
                        break;
                    }
                }

                db.CreateTable<Release>();
                var releasesToAdd = new List<Release>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/release"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var release = new Release
                    {
                        ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                        Name = parts[2],
                        NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                        SortName = parts[2].CleanString(true)
                    };

                    if (release.IsValid)
                    {
                        releasesToAdd.Add(release);
                        if (releasesToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(releasesToAdd, cancellationToken);
                            releaseCountInserted += releasesToAdd.Count;
                            releasesToAdd.Clear();
                        }
                    }

                    if (releaseCountInserted > maxToProcess)
                    {
                        break;
                    }
                }
                
                db.CreateTable<ReleaseCountry>();
                var releaseCountriesToAdd = new List<ReleaseCountry>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/release_country"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var releaseCountry = new ReleaseCountry
                    {
                        ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                        CountryId = SafeParser.ToNumber<long>(parts[1]),
                        DateYear = SafeParser.ToNumber<int>(parts[2]),
                        DateMonth = SafeParser.ToNumber<int>(parts[3]),
                        DateDay = SafeParser.ToNumber<int>(parts[4])
                    };

                    if (releaseCountry.IsValid)
                    {
                        releaseCountriesToAdd.Add(releaseCountry);
                        if (releaseCountriesToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(releaseCountriesToAdd, cancellationToken);
                            releaseCountriesCountInserted += releaseCountriesToAdd.Count;
                            releaseCountriesToAdd.Clear();
                        }
                    }

                    if (releaseCountriesCountInserted > maxToProcess)
                    {
                        break;
                    }
                }                

                db.CreateTable<Tag>();
                var tagsToAdd = new List<Tag>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/tag"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var tag = new Tag
                    {
                        Id = SafeParser.ToNumber<long>(parts[0]),
                        Name = parts[1]
                    };

                    if (tag.IsValid)
                    {
                        tagsToAdd.Add(tag);
                        if (releasesToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(tagsToAdd, cancellationToken);
                            tagCountInserted += tagsToAdd.Count;
                            tagsToAdd.Clear();
                        }
                    }

                    if (tagCountInserted > maxToProcess)
                    {
                        break;
                    }
                }             
                
                db.CreateTable<ReleaseTag>();
                var releaseTagsToAdd = new List<ReleaseTag>();
                await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/mbdump/release_tag"), cancellationToken))
                {
                    var parts = lineFromFile.Split('\t');

                    var releaseTag = new ReleaseTag
                    {
                        ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                        TagId = SafeParser.ToNumber<long>(parts[1])
                    };

                    if (releaseTag.IsValid)
                    {
                        releaseTagsToAdd.Add(releaseTag);
                        if (releaseTagsToAdd.Count >= batchSize)
                        {
                            await db.InsertAllAsync(releaseTagsToAdd, cancellationToken);
                            releaseTagsCountInserted += releaseTagsToAdd.Count;
                            releaseTagsToAdd.Clear();
                        }
                    }

                    if (releaseTagsCountInserted > maxToProcess)
                    {
                        break;
                    }
                }

                await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var setting = await scopedContext
                        .Settings
                        .FirstOrDefaultAsync(x => x.Key == SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    if (setting != null)
                    {
                        setting.Value = (Instant.FromDateTimeUtc(DateTime.UtcNow)).ToString();
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return new OperationResult<bool>
            {
                Data = artistCountInserted > 0 &&
                       releaseCountInserted > 0 &&
                       artistCreditInserted > 0 &&
                       artistCreditNameInserted > 0 && 
                       releaseCountriesCountInserted > 0 &&
                       tagCountInserted > 0 &&
                       releaseTagsCountInserted > 0
            };
        }
    }
}
