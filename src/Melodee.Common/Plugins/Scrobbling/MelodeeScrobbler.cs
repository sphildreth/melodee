using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;

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
            var dbConn = scopedContext.Database.GetDbConnection();

            var sql = """
                      update "Artists" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                      where "Id" = @artistId;
                      update "Albums" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                      where "Id" = @albumId;
                      update "Songs" set "PlayedCount" = "PlayedCount" + 1, "LastPlayedAt" = Now()
                      where "Id" = @songId;
                      """;
            await dbConn.ExecuteAsync(sql,
                    new { artistId = scrobble.ArtistId, albumId = scrobble.AlbumId, songId = scrobble.SongId })
                .ConfigureAwait(false);

            sql = """
                  insert INTO "UserSongs" ("UserId", "SongId", "PlayedCount", "LastPlayedAt", "IsStarred", "IsHated", "Rating", "IsLocked", "SortOrder", "ApiKey", "CreatedAt") 
                  values (@userId, @songId, 1, now(), false, false, 0, false, 0, gen_random_uuid(), now())
                  on CONFLICT("UserId", "SongId") do update
                    set "PlayedCount" = (select "PlayedCount" + 1 from "UserSongs" where "UserId" = @userId and "SongId" = @songId),
                        "LastPlayedAt" = now();
                  """;
            await dbConn.ExecuteAsync(sql, new { userId = user.Id, songId = scrobble.SongId }).ConfigureAwait(false);
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
