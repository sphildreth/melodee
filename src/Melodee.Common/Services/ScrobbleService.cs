using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public class ScrobbleService(
    ILogger logger,
    ICacheManager cacheManager,
    AlbumService albumService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    INowPlayingRepository nowPlayingRepository)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    private bool _initialized;

    private IScrobbler[] _scrobblers = [];

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        _configuration = configuration ??
                         await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

        _scrobblers =
        [
            new MelodeeScrobbler(albumService, ContextFactory, nowPlayingRepository)
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

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, Guid songId, bool isRandomizedScrobble, string playerName, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = true;

        var songIds = await DatabaseSongIdsInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
        if (songIds != null)
        {
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
                    try
                    {
                        var scrobbleResult = await scrobbler.Scrobble(user, scrobble, cancellationToken).ConfigureAwait(false);
                        result &= scrobbleResult.IsSuccess;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "[{Plugin}] threw error with song [{Song}]", scrobbler.DisplayName, songId);
                        result = false;
                        break;
                    }
                }

                Logger.Information("[{ServiceName}] Scrobbled song [{Song}] for User [{User}]",
                    nameof(ScrobbleService),
                    songId.ToString(),
                    user.ToString());
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}
