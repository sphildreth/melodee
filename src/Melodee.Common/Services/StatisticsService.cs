using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Enums;
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
                "Shares",
                await scopedContext.Shares.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                7,
                "share"));              
            
            results.Add(new Statistic(StatisticType.Count,
                "Songs",
                await scopedContext.Songs.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                8,
                "music_note"));     
            
            results.Add(new Statistic(StatisticType.Count,
                "Songs: Played count",
                await scopedContext.Songs.SumAsync(x => x.PlayedCount, cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                9,
                "analytics"));            
           
            results.Add(new Statistic(StatisticType.Count,
                "Users",
                await scopedContext.Users.CountAsync(cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                10,
                "group"));
            
            results.Add(new Statistic(StatisticType.Count,
                "Users: Starred artists",
                await scopedContext.UserArtists.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                11,
                "analytics"));
            
            results.Add(new Statistic(StatisticType.Count,
                "Users: Starred albums",
                await scopedContext.UserAlbums.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                12,
                "analytics"));            
            
            results.Add(new Statistic(StatisticType.Count,
                "Users: Starred songs",
                await scopedContext.UserSongs.CountAsync(x => x.StarredAt != null, cancellationToken)
                    .ConfigureAwait(false),
                null, 
                null, 
                12,
                "analytics"));            
        }

        return new OperationResult<Statistic[]>
        {
            Data = results.ToArray()
        };
    }
}