using System.Globalization;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services;

public sealed class StatisticsService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<Statistic[]>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<Statistic>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();

            results.Add(new Statistic(StatisticType.Count,
                "Albums",
                await scopedContext.Albums.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                1,
                "album"));

            results.Add(new Statistic(StatisticType.Count,
                "Artists",
                await scopedContext.Artists.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                2,
                "artist"));

            results.Add(new Statistic(StatisticType.Count,
                "Contributors",
                await scopedContext.Contributors.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                3,
                "contacts_product"));

            var allGenres = await dbConn.QueryAsync<string>("""
                                                            select distinct "Genres" 
                                                            from 
                                                            (
                                                            	select unnest("Genres") as "Genres"
                                                            	from "Albums"
                                                            	union ALL
                                                            	select  unnest("Genres") as "Genres"
                                                            	from "Songs"
                                                            ) t
                                                            group by "Genres"
                                                            order by "Genres";
                                                            """, cancellationToken).ConfigureAwait(false);
            results.Add(new Statistic(StatisticType.Count,
                "Genres",
                allGenres.Count(),
                null,
                null,
                4,
                "genres"));

            results.Add(new Statistic(StatisticType.Count,
                "Libraries",
                await scopedContext.Libraries.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                5,
                "library_music"));

            results.Add(new Statistic(StatisticType.Count,
                "Playlists",
                await scopedContext.Playlists.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                6,
                "playlist_play"));

            results.Add(new Statistic(StatisticType.Count,
                "Radio Stations",
                await scopedContext.RadioStations.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                7,
                "radio"));            
            
            results.Add(new Statistic(StatisticType.Count,
                "Shares",
                await scopedContext.Shares.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                8,
                "share"));

            results.Add(new Statistic(StatisticType.Count,
                "Songs",
                await scopedContext.Songs.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                9,
                "music_note"));

            results.Add(new Statistic(StatisticType.Count,
                "Songs: Played count",
                await scopedContext.Songs.SumAsync(x => x.PlayedCount, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                10,
                "analytics"));

            results.Add(new Statistic(StatisticType.Count,
                "Users",
                await scopedContext.Users.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                11,
                "group"));

            results.Add(new Statistic(StatisticType.Count,
                "Users: Favorited artists",
                await scopedContext.UserArtists.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                12,
                "analytics"));

            results.Add(new Statistic(StatisticType.Count,
                "Users: Favorited albums",
                await scopedContext.UserAlbums.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                13,
                "analytics"));

            results.Add(new Statistic(StatisticType.Count,
                "Users: Favorited songs",
                await scopedContext.UserSongs.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                14,
                "analytics"));
            
            results.Add(new Statistic(StatisticType.Count,
                "Users: Rated songs",
                await scopedContext.UserSongs.CountAsync(x => x.Rating > 0, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                15,
                "analytics"));            

            results.Add(new Statistic(StatisticType.Information,
                "Total: Song Mb",
                (await scopedContext.Songs.SumAsync(x => x.FileSize, cancellationToken).ConfigureAwait(false)).FormatFileSize(),
                null,
                null,
                16,
                "bar_chart"));

            results.Add(new Statistic(StatisticType.Information,
                "Total: Song Duration",
                (await scopedContext.Songs.SumAsync(x => x.Duration, cancellationToken).ConfigureAwait(false)).ToTimeSpan().ToYearDaysMinutesHours(),
                null,
                "Total song duration in Year:Day:Hour:Minute format.",
                17,
                "bar_chart"));
        }

        return new OperationResult<Statistic[]>
        {
            Data = results.ToArray()
        };
    }

    public async Task<OperationResult<Statistic[]>> GetUserSongStatisticsAsync(Guid userApiKey, CancellationToken cancellationToken = default)
    {
        var results = new List<Statistic>();
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(new Statistic(StatisticType.Count,
                "Your Favorite songs",
                await scopedContext.UserSongs
                    .Where(x => x.User.ApiKey == userApiKey)
                    .CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                1,
                "analytics"));

            results.Add(new Statistic(StatisticType.Count,
                "Your Rated songs",
                await scopedContext.UserSongs
                    .Where(x => x.User.ApiKey == userApiKey)
                    .CountAsync(x => x.Rating > 0, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                2,
                "analytics"));
        }

        return new OperationResult<Statistic[]>
        {
            Data = results.ToArray()
        };
    }
    
    public async Task<OperationResult<Statistic[]>> GetUserAlbumStatisticsAsync(Guid userApiKey, CancellationToken cancellationToken = default)
    {
        var results = new List<Statistic>();
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(new Statistic(StatisticType.Count,
                "Your Favorite albums",
                await scopedContext.UserAlbums
                    .Where(x => x.User.ApiKey == userApiKey)
                    .CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                1,
                "analytics"));
        }

        return new OperationResult<Statistic[]>
        {
            Data = results.ToArray()
        };
    }    
    
    public async Task<OperationResult<Statistic[]>> GetUserArtistStatisticsAsync(Guid userApiKey, CancellationToken cancellationToken = default)
    {
        var results = new List<Statistic>();
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(new Statistic(StatisticType.Count,
                "Your Favorite artists",
                await scopedContext.UserArtists
                    .Where(x => x.User.ApiKey == userApiKey)
                    .CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null,
                null,
                1,
                "analytics"));
        }

        return new OperationResult<Statistic[]>
        {
            Data = results.ToArray()
        };
    }    
}
