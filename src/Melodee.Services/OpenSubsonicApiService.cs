using System.ComponentModel;
using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Utility;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Data;
using SixLabors.ImageSharp;
using SmartFormat;
using License = Melodee.Common.Models.OpenSubsonic.License;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

/// <summary>
/// Handles OpenSubsonic API calls.
/// </summary>
public class OpenSubsonicApiService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    UserService userService,
    ArtistService artistService,
    AlbumService albumService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private Lazy<Task<IMelodeeConfiguration>> Configuration => new Lazy<Task<IMelodeeConfiguration>>(() => settingService.GetMelodeeConfigurationAsync());

    public async Task<LicenseResponse> GetLicense(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        return new LicenseResponse
        {
            IsSuccess = true,
            License = new License(true,
                (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerLicenseEmail) ?? ServiceUser.Instance.Value.Email,
                DateTimeOffset.UtcNow.AddYears(50).ToString("O"),
                DateTimeOffset.UtcNow.AddYears(50).ToString("O")
            ),
            ResponseData = (await DefaultApiResponse())
        };
    }

    public async Task<GetAlbumList2Response> GetAlbumList2Async(GetAlbumListRequest albumListRequest, ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new GetAlbumList2Response
            {
                AlbumList2 = new AlbumList2Wrapper
                {
                    Album = []
                },
                ResponseData = authResponse.ResponseData
            };
        }

        AlbumList2[] data = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sqlParameters = new Dictionary<string, object>();
            var selectSql = """
                            SELECT 
                            cast(a."ApiKey" as varchar(50)) as "Id",
                            a."Name" as "Album",
                            a."Name" as "Title",
                            a."Name" as "Name",
                            'album:' || cast(a."ApiKey" as varchar(50)) as "CoverArt",
                            a."SongCount",
                            a."CreatedAt" as "CreatedRaw",
                            a."Duration"/1000 as "Duration",
                            a."PlayedCount",
                            cast(aa."ApiKey" as varchar(50)) as "ArtistId",
                            aa."Name" as "Artist",
                            DATE_PART('year', a."ReleaseDate"::date) as "Year",
                            SPLIT_PART(a."Genres", '|', 1) as "Genre",
                            a."Genres" as "GenreRaw"
                            FROM "Albums" a 
                            LEFT JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                            """;
            var whereSQL = string.Empty;
            var limitSQL = $"OFFSET {albumListRequest.Offset} ROWS FETCH NEXT {albumListRequest.Size} ROWS ONLY;";
            switch (albumListRequest.Type)
            {
                case ListType.Random:
                    whereSQL = "ORDER BY RANDOM()";
                    break;

                case ListType.Newest:
                    break;
                case ListType.Highest:
                    break;
                case ListType.Frequent:
                    break;
                case ListType.Recent:
                    break;
                case ListType.AlphabeticalByName:
                    break;
                case ListType.AlphabeticalByArtist:
                    break;
                case ListType.Starred:
                    break;
                case ListType.ByYear:
                    break;
                case ListType.ByGenre:
                    break;
            }

            var sql = $"{selectSql} {whereSQL} {limitSQL}";
            data = (await dbConn
                .QueryAsync<AlbumList2>(sql, sqlParameters)
                .ConfigureAwait(false)).ToArray();
        }
        return new GetAlbumList2Response
        {
            IsSuccess = true,
            AlbumList2 = new AlbumList2Wrapper
            {
                Album = data,
            },
            ResponseData = authResponse.ResponseData
        };
    }

    public async Task<GetGenresResponse> GetGenresAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new GetGenresResponse
            {
                Genres = new GetGenresResponseWrapper
                {
                    Genre = []
                },
                ResponseData = authResponse.ResponseData
            };
        }

        var data = new List<Genre>();
        
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var allGenres = await dbConn.QueryAsync<string>("""
                                                              select genre
                                                              from
                                                              (
                                                                  select genre
                                                                  from "Albums", unnest("Genres") as genre 
                                                                  union ALL
                                                                  select genre
                                                                  from "Songs", unnest("Genres") as genre 
                                                              ) t
                                                              group by genre
                                                              """, cancellationToken).ConfigureAwait(false);
            var songGenres = await dbConn.QueryAsync<Genre>($"select genre as Value, count(1) as SongCount from \"Songs\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false);
            var albumGenres = await dbConn.QueryAsync<Genre>($"select genre as Value, count(1) as AlbumCount from \"Albums\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false);

            foreach (var genre in allGenres)
            {
                var songGenre = songGenres.FirstOrDefault(x => x.Value == genre);
                var albumGenre = albumGenres.FirstOrDefault(x => x.Value == genre);
                data.Add(new Genre
                    {
                        Value = genre,
                        SongCount = songGenre?.SongCount ?? 0,
                        AlbumCount = albumGenre?.AlbumCount ?? 0
                    }
                );
            }
        }

        return new GetGenresResponse
        {
            IsSuccess = true,
            Genres = new GetGenresResponseWrapper
            {
                Genre = data.ToArray()
            },
            ResponseData = authResponse.ResponseData
        };
    }
    
    
    public async Task<ResponseModel<ApiResponse>> AuthenticateSubsonicApiAsync(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(apiApiRequest.Username, nameof(apiApiRequest.Username));
        Guard.Against.NullOrWhiteSpace(apiApiRequest.Salt, nameof(apiApiRequest.Salt));
        Guard.Against.NullOrWhiteSpace(apiApiRequest.Token, nameof(apiApiRequest.Token));

        bool result = false;

        var user = await userService.GetByUsernameAsync(apiApiRequest.Username, cancellationToken).ConfigureAwait(false);
        if (!user.IsSuccess || user.Data.IsLocked)
        {
            Logger.Warning("Locked user [{Username}] attempted to authenticate with [{Client}]", apiApiRequest.Username, apiApiRequest.ApiRequestPlayer);
        }
        else
        {
            var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
            var usersPassword = user.Data.Decrypt(user.Data.PasswordEncrypted, configuration);
            var userMd5 = HashHelper.CreateMd5($"{usersPassword}{apiApiRequest.Salt}");
            if (string.Equals(userMd5, apiApiRequest.Token, StringComparison.InvariantCultureIgnoreCase))
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                result = true;
                _ = Task.Run(() =>
                {
                     using (var scopedContext = ContextFactory.CreateDbContext())
                     {
                        scopedContext.Users.Where(x => x.Id == user.Data.Id)
                            .ExecuteUpdate(setters => setters.SetProperty(x => x.LastActivityAt, now).SetProperty(x => x.LastLoginAt, now));
                     }
                });
            }
        }

        return new ResponseModel<ApiResponse>
        {
            IsSuccess = result,
            ResponseData = (await NewApiResponse(result, result ? null : Error.AuthError))
        };
    }

    private Task<ApiResponse> DefaultApiResponse()
        => NewApiResponse(true, null);

    private async Task<ApiResponse> NewApiResponse(bool isOk, Error? error)
    {
        return new ApiResponse
        (
            isOk ? "ok" : "failed",
            (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerSupportedVersion) ?? throw new InvalidOperationException(),
            (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerType) ?? throw new InvalidOperationException(),
            (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerVersion) ?? throw new InvalidOperationException(),
            true,
            error
        );
    }
}
