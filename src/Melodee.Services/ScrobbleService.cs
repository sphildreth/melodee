using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;
using Melodee.Plugins.Scrobbling;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using ServiceStack;

namespace Melodee.Services;

public class ScrobbleService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISettingService settingService,
    INowPlayingRepository nowPlayingRepository,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    private bool _initialized;

    private IScrobbler[] _scrobblers = [];

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken cancellationToken = default)
    {
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(cancellationToken).ConfigureAwait(false);

        _scrobblers =
        [
            new MelodeeScrobbler(ContextFactory, nowPlayingRepository, _configuration)
            {
                IsEnabled = true
            },
            new LastFmScrobbler(_configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.ScrobblingLastFmEnabled)
            }
        ];

        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Scrobble service is not initialized.");
        }
    }

    /// <summary>
    ///     Returns the actively playing songs and user infos.
    /// </summary>
    public Task<OperationResult<NowPlayingInfo[]>> GetNowPlaying(CancellationToken cancellationToken = default)
    {
        return nowPlayingRepository.GetNowPlayingAsync(cancellationToken);
    }

    public async Task<OperationResult<bool>> NowPlaying(UserInfo user, Guid id, double? time, string playerName, CancellationToken cancellationToken = default)
    {
        var result = true;
        var databaseSongScrobbleInfo = await DatabaseSongScrobbleInfoForSongApiKey(id, cancellationToken).ConfigureAwait(false);
        if (databaseSongScrobbleInfo != null)
        {
            var scrobble = new ScrobbleInfo
            (
                databaseSongScrobbleInfo.SongApiKey,
                databaseSongScrobbleInfo.ArtistId,
                databaseSongScrobbleInfo.AlbumId,
                databaseSongScrobbleInfo.SongId,
                databaseSongScrobbleInfo.SongTitle,
                databaseSongScrobbleInfo.ArtistName,
                false,
                databaseSongScrobbleInfo.AlbumTitle,
                databaseSongScrobbleInfo.SongDuration.ToSeconds(),
                databaseSongScrobbleInfo.SongMusicBrainzId,
                databaseSongScrobbleInfo.SongNumber,
                null,
                Instant.FromDateTimeUtc(DateTime.UtcNow),
                playerName
            )
            {
                LastScrobbledAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            foreach (var scrobbler in _scrobblers.OrderBy(x => x.SortOrder).Where(x => x.IsEnabled))
            {
                var nowPlayingResult = await scrobbler.NowPlaying(user, scrobble, cancellationToken).ConfigureAwait(false);
                result &= nowPlayingResult.IsSuccess;
            }            
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, Guid songId, double? time, bool isRandomizedScrobble, string playerName, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = true;

        var songIds = await DatabaseSongIdsInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
        if (songIds != null)
        {
            //TODO what about Contributors? 

            var databaseSongScrobbleInfo = await DatabaseSongScrobbleInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
            if (databaseSongScrobbleInfo != null)
            {
                var scrobble = new ScrobbleInfo
                (
                    databaseSongScrobbleInfo.SongApiKey,
                    databaseSongScrobbleInfo.ArtistId,
                    databaseSongScrobbleInfo.AlbumId,
                    databaseSongScrobbleInfo.SongId,
                    databaseSongScrobbleInfo.SongTitle,
                    databaseSongScrobbleInfo.ArtistName,
                    isRandomizedScrobble,
                    databaseSongScrobbleInfo.AlbumTitle,
                    databaseSongScrobbleInfo.SongDuration.ToSeconds(),
                    databaseSongScrobbleInfo.SongMusicBrainzId,
                    databaseSongScrobbleInfo.SongNumber,
                    null,
                    Instant.FromDateTimeUtc(DateTime.UtcNow),
                    playerName
                )
                {
                    LastScrobbledAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                };
                foreach (var scrobbler in _scrobblers.OrderBy(x => x.SortOrder).Where(x => x.IsEnabled))
                {
                    var scrobbleResult = await scrobbler.Scrobble(user, scrobble, cancellationToken).ConfigureAwait(false);
                    result &= scrobbleResult.IsSuccess;
                }
                Logger.Information("[{ServiceName}] Scrobbled song [{SongId}] for User [{User}]", nameof(ScrobbleService), user.ToString(), songId.ToString());
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}
