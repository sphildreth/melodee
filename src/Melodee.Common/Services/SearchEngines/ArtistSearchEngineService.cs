using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
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
using Microsoft.VisualBasic.CompilerServices;
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

    public async Task<PagedResult<ArtistSearchResult>> DoSearchAsync(ArtistQuery query, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = new List<ArtistSearchResult>();

        var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        long operationTime = 0;
        int totalCount = 0;

        using (Operation.At(LogEventLevel.Debug).Time("[{Name}] DoSearchAsync [{DirectoryInfo}]",
                   nameof(ArtistSearchEngineService), query))
        {
            
            // See if found in DbContext if not then query plugins, add to context and return results
            await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var artists = await scopedContext
                    .Artists.Include(x => x.Albums)
                    .Where(x => x.NameNormalized == query.NameNormalized || (x.MusicBrainzId != null && x.MusicBrainzId == query.MusicBrainzIdValue))
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
                    result.Add(new ArtistSearchResult
                    {
                        AmgId = artist.AmgId,
                        DiscogsId = artist.DiscogsId,
                        FromPlugin = nameof(ArtistSearchEngineService),
                        ItunesId = artist.ItunesId,
                        LastFmId = artist.LastFmId,
                        MusicBrainzId = artist.MusicBrainzId,
                        Name = artist.Name,
                        Rank = artist.Rank,
                        SortName = artist.SortName,
                        SpotifyId = artist.SpotifyId,
                        UniqueId = artist.Id,
                        WikiDataId = artist.WikiDataId,
                        Releases = artist.Albums?.Select(x => x.ToAlbumSearchResult(artist, nameof(ArtistSearchEngineService))).ToArray()
                    });
                    Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Found artist [{artist}] in database for query [{query}].");
                }
                else
                {
                    foreach (var plugin in _artistSearchEnginePlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        var pluginResult = await plugin.DoArtistSearchAsync(query, maxResultsValue, cancellationToken).ConfigureAwait(false);
                        if (pluginResult is { IsSuccess: true, Data: not null })
                        {
                            result.AddRange(pluginResult.Data);
                            totalCount += pluginResult.TotalCount;
                            operationTime += pluginResult.OperationTime ?? 0;
                        }

                        if (plugin.StopProcessing || result.Count >= maxResultsValue)
                        {
                            break;
                        }
                    }

                    if (result.Count > 0)
                    {
                        var rr = result.First();
                        var newArtist = new ArtistSearchResult
                        {
                            AmgId = rr.AmgId,
                            DiscogsId = rr.DiscogsId,
                            FromPlugin = nameof(ArtistSearchEngineService),
                            ItunesId = rr.ItunesId,
                            LastFmId = rr.LastFmId,
                            MusicBrainzId = rr.MusicBrainzId,
                            Name = rr.Name,
                            Rank = 1,
                            SortName = rr.SortName,
                            SpotifyId = rr.SpotifyId,
                            WikiDataId = rr.WikiDataId,
                        };
                        newArtist = result.Aggregate(newArtist, (current, r) => current with
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
                        var combinedNewArtistReleases = new List<AlbumSearchResult>();
                        foreach (var ar in result)
                        {
                            foreach (var arRelease in ar.Releases ?? [])
                            {
                                var seenAlbumRelease = combinedNewArtistReleases.FirstOrDefault(x => x.Year == arRelease.Year && x.NameNormalized == arRelease.NameNormalized);
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
                        newArtist.Releases = combinedNewArtistReleases.Where(x => x.AlbumType is AlbumType.Album or AlbumType.EP).ToArray();
                        result.Clear();
                        
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
                            result.Add(new ArtistSearchResult
                            {
                                AmgId = artist.AmgId,
                                DiscogsId = artist.DiscogsId,
                                FromPlugin = nameof(ArtistSearchEngineService),
                                ItunesId = artist.ItunesId,
                                LastFmId = artist.LastFmId,
                                MusicBrainzId = artist.MusicBrainzId,
                                Name = artist.Name,
                                Rank = 1,
                                SortName = artist.SortName,
                                SpotifyId = artist.SpotifyId,
                                UniqueId = artist.Id,
                                WikiDataId = artist.WikiDataId,
                            });
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
                                albums = newArtist.Releases.Select(x => new Album
                                {
                                    AlbumType = (int)x.AlbumType,
                                    Artist = newDbArtist,
                                    ArtistId = newDbArtist.Id,
                                    SortName = x.SortName,
                                    Name = x.Name,
                                    NameNormalized = x.NameNormalized,
                                    Year = SafeParser.ToDateTime(x.ReleaseDate)?.Year ?? 0,
                                    MusicBrainzId = x.MusicBrainzId,
                                    MusicBrainzReleaseGroupId = x.MusicBrainzResourceGroupId,
                                    SpotifyId = x.SpotifyId,
                                    CoverUrl = x.CoverUrl
                                }).ToArray();
                                
                                newDbArtist.Albums = albums;                                
                            }                            
                            scopedContext.Artists.Add(newDbArtist);
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            newArtist = newArtist with { UniqueId = newDbArtist.Id, Releases = albums.Select(x => x.ToAlbumSearchResult(x.Artist, nameof(ArtistSearchEngineService))).ToArray()};
                            result.Add(newArtist);
                            Trace.WriteLine($"[{nameof(ArtistSearchEngineService)}] Added artist [{newArtist}] with [{albums.Length}] albums to database.");
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
            Data = result.OrderByDescending(x => x.Rank).ThenBy(x => x.SortName).ToArray()
        };
    }
}
