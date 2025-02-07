using System.Diagnostics;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Dapper.SqliteHandlers;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Extension;
using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.SearchEngine.Spotify;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using ServiceStack;
using Album = Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Album;
using Artist = Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData.Artist;

namespace Melodee.Common.Services.SearchEngines;

public class ArtistSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    SettingService settingService,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> melodeeDbContextFactory,
    IDbContextFactory<ArtistSearchEngineServiceDbContext> artistSearchEngineServiceDbContextFactory,
    IMusicBrainzRepository musicBrainzRepository)
    : ServiceBase(logger, cacheManager, melodeeDbContextFactory)
{
    private IArtistSearchEnginePlugin[] _artistSearchEnginePlugins = [];
    private IArtistTopSongsSearchEnginePlugin[] _artistTopSongsSearchEnginePlugins = [];
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private bool _initialized;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken cancellationToken = default)
    {
        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

        _artistSearchEnginePlugins =
        [
            new MelodeeArtistSearchEnginPlugin(ContextFactory),
            new MusicBrainzArtistSearchEnginePlugin(musicBrainzRepository)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled)
            },
            new Spotify(Log.Logger, _configuration, settingService)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.SearchEngineSpotifyEnabled)
            }
        ];

        _artistTopSongsSearchEnginePlugins =
        [
            new MelodeeArtistSearchEnginPlugin(ContextFactory),
            new Spotify(Log.Logger, _configuration, settingService)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.SearchEngineSpotifyEnabled)
            }
        ];

        await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            await scopedContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"{nameof(ArtistSearchEngineService)} is not initialized.");
        }
    }
    
    public async Task<PagedResult<Artist>> ListAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        SqlMapper.ResetTypeHandlers();
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new TimeSpanHandler());
        
        int albumCount;
        Artist[] artists = [];
        await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Artists\"");
            albumCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                
                var sqlStartFragment = """
                                       SELECT a."Id", a."Name", a."NameNormalized", a."ItunesId", a."AmgId", a."DiscogsId", a."WikiDataId", 
                                              a."MusicBrainzId", a."LastFmId", a."SpotifyId", a."SortName" 
                                       FROM "Artists" a
                                       """;
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment, "a");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                artists = (await dbConn
                    .QueryAsync<Artist>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
                
                foreach(var artist in artists )
                {
                    artist.AlbumCount = await dbConn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Albums\" WHERE \"ArtistId\" = @ArtistId", new { ArtistId = artist.Id });
                }
            }
        }
        SqlMapper.ResetTypeHandlers();
        
        return new PagedResult<Artist>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = artists
        };
    }    

    public async Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(string artistName, int? artistId, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);
        int totalCount = 0;
        long operationTime = 0;

        var artistIdValue = artistId;
        if (artistIdValue == null)
        {
            var searchResult = await DoSearchAsync(new ArtistQuery { Name = artistName }, maxResults, cancellationToken).ConfigureAwait(false);
            artistIdValue = searchResult.Data.FirstOrDefault(x => x.Id != null)?.Id;
        }

        if (artistIdValue == null)
        {
            return new PagedResult<SongSearchResult>([$"No artist found for [{artistName}]"])
            {
                Data = []
            };
        }

        var result = new List<SongSearchResult>();

        foreach (var plugin in _artistTopSongsSearchEnginePlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var pluginResult = await plugin.DoArtistTopSongsSearchAsync(artistIdValue.Value, maxResultsValue, cancellationToken).ConfigureAwait(false);
            if (pluginResult is { IsSuccess: true, Data: not null })
            {
                result.AddRange(pluginResult.Data);
                totalCount += pluginResult.TotalCount;
                operationTime += pluginResult.OperationTime ?? 0;
            }

            if (result.Count > maxResultsValue)
            {
                break;
            }
        }

        return new PagedResult<SongSearchResult>
        {
            OperationTime = operationTime,
            CurrentPage = 1,
            TotalCount = totalCount,
            TotalPages = 1,
            Data = result.OrderBy(x => x.SortOrder).ToArray()
        };
    }

    private async Task<ArtistSearchResult?> GetArtistFromSearchProviders(ArtistQuery query, int maxResultsValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var pluginsResult = new List<ArtistSearchResult>();
            foreach (var plugin in _artistSearchEnginePlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                // Don't limit the number of results from the search engine as they will be put into the local database and limited on result to method call.
                var pluginResult = await plugin.DoArtistSearchAsync(query, int.MaxValue, cancellationToken).ConfigureAwait(false);
                if (pluginResult is { IsSuccess: true, Data: not null })
                {
                    pluginsResult.AddRange(pluginResult.Data);
                }
                if (plugin.StopProcessing || pluginsResult.Count >= maxResultsValue)
                {
                    break;
                }
            }
            if (pluginsResult.Count > 0)
            {
                var artistsFromSearchResult = new List<ArtistSearchResult>();
                foreach (var pluginResult in pluginsResult)
                {
                    var artistFromSearchResult = new ArtistSearchResult
                    {
                        AmgId = pluginResult.AmgId,
                        DiscogsId = pluginResult.DiscogsId,
                        FromPlugin = nameof(ArtistSearchEngineService),
                        ItunesId = pluginResult.ItunesId,
                        LastFmId = pluginResult.LastFmId,
                        MusicBrainzId = pluginResult.MusicBrainzId,
                        Name = pluginResult.Name,
                        Rank = 1,
                        SortName = pluginResult.SortName,
                        SpotifyId = pluginResult.SpotifyId,
                        WikiDataId = pluginResult.WikiDataId,
                    };
                    artistFromSearchResult = pluginsResult.Aggregate(artistFromSearchResult, (current, r) => current with
                    {
                        AmgId = current.AmgId ?? r.AmgId,
                        DiscogsId = current.DiscogsId ?? r.DiscogsId,
                        ItunesId = current.ItunesId ?? r.ItunesId,
                        LastFmId = current.LastFmId ?? r.LastFmId,
                        Name = current.Name.Nullify() ?? r.Name,
                        MusicBrainzId = current.MusicBrainzId ?? r.MusicBrainzId,
                        Rank = current.Rank + 1,
                        SpotifyId = current.SpotifyId ?? r.SpotifyId,
                        WikiDataId = current.WikiDataId ?? r.WikiDataId
                    });
                    if (artistFromSearchResult.SpotifyId == null && artistFromSearchResult.MusicBrainzId == null)
                    {
                        Logger.Warning("[{Name}]:[{MethodName}] unable to find artist for query [{Query}].",
                            nameof(ArtistSearchEngineService),
                            nameof(GetArtistFromSearchProviders),
                            query);
                        return null;
                    }

                    var combinedNewArtistReleases = new List<AlbumSearchResult>();
                    foreach (var ar in pluginsResult)
                    {
                        foreach (var arRelease in ar.Releases ?? [])
                        {
                            var seenAlbumRelease = combinedNewArtistReleases
                                .OrderBy(x => x.Year)
                                .FirstOrDefault(x => x.Year == arRelease.Year && x.NameNormalized == arRelease.NameNormalized);
                            if (seenAlbumRelease == null)
                            {
                                combinedNewArtistReleases.Add(arRelease);
                            }
                            else
                            {
                                seenAlbumRelease.MusicBrainzId ??= arRelease.MusicBrainzId;
                                seenAlbumRelease.MusicBrainzResourceGroupId ??= arRelease.MusicBrainzResourceGroupId;
                                seenAlbumRelease.SpotifyId ??= arRelease.SpotifyId;
                                seenAlbumRelease.CoverUrl ??= arRelease.CoverUrl;
                            }
                        }
                    }
                    artistFromSearchResult.Releases = combinedNewArtistReleases.Where(x => x.AlbumType is AlbumType.Album or AlbumType.EP).ToArray();
                    artistsFromSearchResult.Add(artistFromSearchResult);
                }

                foreach (var artistFromSearchResult in artistsFromSearchResult)
                {
                    if (artistFromSearchResult.Releases?.Length > 0 && query.AlbumKeyValues?.Length > 0)
                    {
                        foreach(var queryAlbum in query.AlbumKeyValues )
                        {
                            var matchingAlbumsOnName = artistFromSearchResult.Releases.Where(x => x.NameNormalized == queryAlbum.Value).ToArray();
                            if (matchingAlbumsOnName.Any())
                            {
                                artistFromSearchResult.Rank += matchingAlbumsOnName.Length;
                                var matchingAlbumsOnYear = matchingAlbumsOnName.Where(x => x.Year.ToString() == queryAlbum.Key).ToArray();
                                if (matchingAlbumsOnYear.Any())
                                {
                                    artistFromSearchResult.Rank += matchingAlbumsOnYear.Length;
                                }
                            }
                        }
                        artistFromSearchResult.Releases = artistFromSearchResult.Releases.OrderByDescending(x => x.Rank).ToArray();
                    }
                }
                var newArtist = artistsFromSearchResult.OrderByDescending(x => x.Rank).FirstOrDefault();

                Logger.Debug("[{Name}]:[{MethodName}] artist for query [{Query}] return [{ArtistId}] with [{AlbumCount}] albums.",
                    nameof(ArtistSearchEngineService),
                    nameof(GetArtistFromSearchProviders),
                    query,
                    newArtist?.ToString(),
                    newArtist?.Releases?.Length
                );                
                return newArtist;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{Name}] GetArtistFromSearchProviders [{Query}]", nameof(ArtistSearchEngineService), query);
        }

        return null;
    }
    
    public async Task<PagedResult<ArtistSearchResult>> DoSearchAsync(ArtistQuery query, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = new List<ArtistSearchResult>();

        var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        long operationTime = 0;
        int totalCount = 0;

        using (Operation.At(LogEventLevel.Debug).Time("[{Name}] DoSearchAsync [{Query}]", nameof(ArtistSearchEngineService), query))
        {
            
            // See if found in DbContext if not then query plugins, add to context and return results
            await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var artists = await scopedContext
                    .Artists.Include(x => x.Albums)
                    .Where(x => x.NameNormalized == query.NameNormalized || 
                                (x.MusicBrainzId != null && query.MusicBrainzId != null && x.MusicBrainzId == query.MusicBrainzIdValue) || 
                                (x.SpotifyId != null && query.SpotifyId != null && x.SpotifyId == query.SpotifyId))
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
               
                if (artists.Length > 0 && query.AlbumKeyValues?.Length > 0)
                {
                    foreach (var ar in artists)
                    {
                        // If any album is given then rank artist if any album matches 
                        foreach (var album in ar.Albums)
                        {
                            foreach (var albumKey in query.AlbumKeyValues)
                            {
                                var isAlbumMatch = album.Year.ToString() == albumKey.Key && album.NameNormalized == albumKey.Value;
                                if (isAlbumMatch)
                                {
                                    ar.Rank++;
                                }
                            }
                        }
                    }
                }
                
                var artist = artists.OrderByDescending(x => x.Rank).FirstOrDefault();
               
                if (artist != null)
                {
                    if (artist.Albums.Count == 0)
                    {
                        // Artist in local db doesn't have any albums refresh get and update
                        Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] artist [{artist.NameNormalized}] has no albums. Refreshing from search engine.");
                        var newArtist = await GetArtistFromSearchProviders(query, maxResultsValue, cancellationToken).ConfigureAwait(false);
                        if (newArtist?.Releases?.Length > 0)
                        {
                            var albumsToAdd = new List<Album>();
                            foreach (var ar in newArtist.Releases)
                            {
                                var newAlbum = new Album
                                {
                                    AlbumType = (int)ar.AlbumType,
                                    Artist = artist,
                                    ArtistId = artist.Id,
                                    SortName = ar.SortName,
                                    Name = ar.Name,
                                    NameNormalized = ar.NameNormalized,
                                    Year = SafeParser.ToDateTime(ar.ReleaseDate)?.Year ?? 0,
                                    MusicBrainzId = ar.MusicBrainzId,
                                    MusicBrainzReleaseGroupId = ar.MusicBrainzResourceGroupId,
                                    SpotifyId = ar.SpotifyId,
                                    CoverUrl = ar.CoverUrl
                                };

                                var alreadyInList = albumsToAdd.FirstOrDefault(x => x.NameNormalized == newAlbum.NameNormalized && x.Year == newAlbum.Year);
                                if (alreadyInList == null)
                                {
                                    albumsToAdd.Add(newAlbum);
                                }
                                else
                                {
                                    alreadyInList!.MusicBrainzId ??= alreadyInList?.MusicBrainzId;
                                    alreadyInList!.MusicBrainzReleaseGroupId ??= alreadyInList?.MusicBrainzReleaseGroupId;
                                    alreadyInList!.SpotifyId ??= alreadyInList?.SpotifyId;
                                }
                            }
                            artist.Albums = albumsToAdd.ToArray();
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Updated existing artist [{artist}] added [{artist.Albums.Count}] albums.");        
                            var artistId = artist.Id;
                            artist = await scopedContext
                                .Artists
                                .Include(x => x.Albums)
                                .FirstAsync(x => x.Id == artistId, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    result.Add(artist.ToArtistSearchResult(nameof(ArtistSearchEngineService)));
                    Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Found artist [{artist}] in database for query [{query}].");
                }
                else
                {
                    var newArtist = await GetArtistFromSearchProviders(query, maxResultsValue, cancellationToken).ConfigureAwait(false);
                    if(newArtist != null)
                    {
                        var nameNormalized = newArtist.Name.ToNormalizedString() ?? newArtist.Name;
                        
                        artist = await scopedContext
                            .Artists
                            .Where(x => x.NameNormalized == nameNormalized || 
                                             (x.AmgId != null && x.AmgId == newArtist.AmgId) ||
                                             (x.DiscogsId != null && x.DiscogsId == newArtist.DiscogsId) ||
                                             (x.ItunesId != null && x.ItunesId == newArtist.ItunesId) ||
                                             (x.LastFmId != null && x.LastFmId == newArtist.LastFmId) ||
                                             (x.MusicBrainzId != null && x.MusicBrainzId == newArtist.MusicBrainzId) ||
                                             (x.SpotifyId != null && x.SpotifyId == newArtist.SpotifyId) ||
                                             (x.WikiDataId != null && x.WikiDataId == newArtist.WikiDataId))
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                        
                        if (artist != null)
                        {
                            result.Add(artist.ToArtistSearchResult(nameof(ArtistSearchEngineService)));
                            Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Found artist [{artist}] in database for query [{query}].");
                        }
                        else
                        {  
                            var newDbArtist = new Artist
                            {
                                AmgId = newArtist.AmgId,
                                DiscogsId = newArtist.DiscogsId,
                                ItunesId = newArtist.ItunesId,
                                LastFmId = newArtist.LastFmId,
                                MusicBrainzId = newArtist.MusicBrainzId,
                                Name = newArtist.Name,
                                NameNormalized = newArtist.Name.ToNormalizedString() ?? newArtist.Name,
                                SortName = newArtist.SortName ?? newArtist.Name,
                                SpotifyId = newArtist.SpotifyId,
                                WikiDataId = newArtist.WikiDataId
                            };
                            Album[] albums = [];
                            if (newArtist.Releases?.Length > 0)
                            {
                                var albumsToAdd = new List<Album>();
                                foreach (var ar in newArtist.Releases)
                                {
                                    var newAlbum = new Album
                                    {
                                        AlbumType = (int)ar.AlbumType,
                                        Artist = newDbArtist,
                                        ArtistId = newDbArtist.Id,
                                        SortName = ar.SortName,
                                        Name = ar.Name,
                                        NameNormalized = ar.NameNormalized,
                                        Year = SafeParser.ToDateTime(ar.ReleaseDate)?.Year ?? 0,
                                        MusicBrainzId = ar.MusicBrainzId,
                                        MusicBrainzReleaseGroupId = ar.MusicBrainzResourceGroupId,
                                        SpotifyId = ar.SpotifyId,
                                        CoverUrl = ar.CoverUrl
                                    };

                                    var alreadyInList = albumsToAdd.FirstOrDefault(x => x.NameNormalized == newAlbum.NameNormalized && x.Year == newAlbum.Year);
                                    if (alreadyInList == null)
                                    {
                                        albumsToAdd.Add(newAlbum);
                                    }
                                    else
                                    {
                                        alreadyInList!.MusicBrainzId ??= alreadyInList?.MusicBrainzId;
                                        alreadyInList!.MusicBrainzReleaseGroupId ??= alreadyInList?.MusicBrainzReleaseGroupId;
                                        alreadyInList!.SpotifyId ??= alreadyInList?.SpotifyId;
                                    }
                                }
                                newDbArtist.Albums = albumsToAdd.ToArray();                              
                            }                            
                            scopedContext.Artists.Add(newDbArtist);
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            newArtist = newArtist with { UniqueId = newDbArtist.Id, Releases = albums.Select(x => x.ToAlbumSearchResult(x.Artist, nameof(ArtistSearchEngineService))).ToArray()};
                            result.Add(newArtist);
                            Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Added artist [{newArtist}] with [{newDbArtist.Albums.Count}] albums to database.");
                        }
                    }
                }
            }
        }

        return new PagedResult<ArtistSearchResult>
        {
            OperationTime = operationTime,
            CurrentPage = 1,
            TotalCount = totalCount,
            TotalPages = 1,
            Data = result?.OrderByDescending(x => x.Rank).ThenBy(x => x.SortName).ToArray() ?? []
        };
    }

    public async Task<OperationResult<bool>> RefreshArtistAlbums(Artist[] selectedArtists, CancellationToken cancellationToken = default)
    {
        var result = false;

        await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var artist in selectedArtists)
            {
                var dbArtist = await scopedContext
                    .Artists
                    .FirstOrDefaultAsync(x => x.Id == artist.Id, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                dbArtist?.Albums.Clear();
            }
            var now = DateTime.UtcNow;
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var artist in selectedArtists)
            {
                await DoSearchAsync(new ArtistQuery { Name = artist.Name }, null, cancellationToken).ConfigureAwait(false);
                var dbArtist = await scopedContext
                    .Artists
                    .FirstOrDefaultAsync(x => x.Id == artist.Id, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (dbArtist != null)
                {
                    dbArtist.LastRefreshed = now;
                }
            }
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            result = true;
        }
        return new OperationResult<bool>
        {
            Data = result
        };
    }
}
