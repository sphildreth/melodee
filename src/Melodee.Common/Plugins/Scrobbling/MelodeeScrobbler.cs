using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Plugins.Scrobbling;

public class MelodeeScrobbler(
    AlbumService albumService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    INowPlayingRepository nowPlayingRepository) : IScrobbler
{
    public bool StopProcessing { get; } = false;
    public string Id => "D8A07387-87DF-4136-8D3E-C59EABEB501F";

    public string DisplayName => nameof(MelodeeScrobbler);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<bool>> NowPlaying(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken cancellationToken = default)
    {
        await nowPlayingRepository.AddOrUpdateNowPlayingAsync(new NowPlayingInfo(user, scrobble), cancellationToken)
            .ConfigureAwait(false);
        return new OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<OperationResult<bool>> Scrobble(UserInfo user, ScrobbleInfo scrobble,
        CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            // Update Artist played count and last played time using ExecuteUpdateAsync for performance
            await scopedContext.Artists
                .Where(a => a.Id == scrobble.ArtistId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);

            // Update Album played count and last played time using ExecuteUpdateAsync for performance
            await scopedContext.Albums
                .Where(a => a.Id == scrobble.AlbumId)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);

            // Update Song played count and last played time using ExecuteUpdateAsync for performance
            await scopedContext.Songs
                .Where(s => s.Id == scrobble.SongId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);

            // Handle UserSong upsert logic - first try to update, if no rows affected then insert
            var updatedRows = await scopedContext.UserSongs
                .Where(us => us.UserId == user.Id && us.SongId == scrobble.SongId)
                .ExecuteUpdateAsync(us => us
                    .SetProperty(p => p.PlayedCount, p => p.PlayedCount + 1)
                    .SetProperty(p => p.LastPlayedAt, now), cancellationToken)
                .ConfigureAwait(false);

            if (updatedRows == 0)
            {
                // No existing UserSong found, create new one
                var newUserSong = new UserSong
                {
                    UserId = user.Id,
                    SongId = scrobble.SongId,
                    PlayedCount = 1,
                    LastPlayedAt = now,
                    IsStarred = false,
                    IsHated = false,
                    Rating = 0,
                    IsLocked = false,
                    SortOrder = 0,
                    ApiKey = Guid.NewGuid(),
                    CreatedAt = now
                };
                
                scopedContext.UserSongs.Add(newUserSong);
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await nowPlayingRepository
                .RemoveNowPlayingAsync(SafeParser.Hash(user.ApiKey.ToString(), scrobble.SongApiKey.ToString()),
                    cancellationToken)
                .ConfigureAwait(false);

            var album = await albumService.GetAsync(scrobble.AlbumId, cancellationToken).ConfigureAwait(false);
            if (album.IsSuccess)
            {
                albumService.ClearCache(album.Data!);
            }
        }

        return new OperationResult<bool>
        {
            Data = true
        };
    }
}
