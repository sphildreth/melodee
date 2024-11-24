using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Plugins.Scrobbling;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

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
            new MelodeeScrobbler(_configuration)
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
        var databaseSongScrobbleInfo = await DatabaseSongScrobbleInfoForSongApiKey(id, cancellationToken).ConfigureAwait(false);
        if (databaseSongScrobbleInfo != null)
        {
            var scrobble = new ScrobbleInfo
            (
                databaseSongScrobbleInfo.SongApiKey,
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
            );
            scrobble.LastScrobbledAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await nowPlayingRepository.AddOrUpdateNowPlayingAsync(new NowPlayingInfo(user, scrobble), cancellationToken).ConfigureAwait(false);
        }

        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, Guid songId, double? time, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;

        var songIds = await DatabaseSongIdsInfoForSongApiKey(songId, cancellationToken).ConfigureAwait(false);
        if (songIds != null)
        {
            // TODO what about Contributors? 


            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();

                var sql = """
                          update "Artists" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                          where "Id" = @artistId;
                          update "Albums" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                          where "Id" = @albumId;
                          update "Songs" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                          where "Id" = @songId;
                          """;
                await dbConn.ExecuteAsync(sql, new { artistId = songIds.AlbumArtistId, albumId = songIds.AlbumId, songId = songIds.SongId }).ConfigureAwait(false);

                await artistService.ClearCacheAsync(songIds.AlbumArtistId, cancellationToken).ConfigureAwait(false);
                await albumService.ClearCacheAsync(songIds.AlbumId, cancellationToken).ConfigureAwait(false);
                await songService.ClearCacheAsync(songIds.SongId, cancellationToken).ConfigureAwait(false);

                sql = """
                      merge into "UserSongs" us
                      using (select "UserId", "SongId" from "UserSongs" where "UserId" = @userId and "SongId" = @songId) as uss
                      on uss."UserId" = us."UserId" and uss."SongId" = us."SongId"
                      when matched then 
                      	update set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = now()
                      when not matched then 
                      	insert ("UserId", "SongId", "PlayedCount", "LastPlayedAt")
                      	values (@userId, @songId, 1, now());
                      """;
                await dbConn.ExecuteAsync(sql, new { userId = user.Id, songId = songIds.SongId }).ConfigureAwait(false);

                result = true;
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}
