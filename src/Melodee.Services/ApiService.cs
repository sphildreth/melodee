using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
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
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class ApiService(
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

    public async Task<ResponseModel<ApiResponse>> GetLicense(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        return new LicenseResponse
        {
            License = new License(true,
                (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerLicenseEmail) ?? ServiceUser.Instance.Value.Email,
                DateTimeOffset.UtcNow.AddYears(50).ToString("O"),
                DateTimeOffset.UtcNow.AddYears(50).ToString("O")
            ),
            ResponseData = (await DefaultApiResponse())
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
                // TODO use background worker for this  https://stackoverflow.com/a/482210/74071
                // _ = Task.Run(async () =>
                // {
                //     await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                //     {
                //         var dbUser = await scopedContext
                //             .Users
                //             .SingleAsync(x => x.Id == user.Data.Id, cancellationToken)
                //             .ConfigureAwait(false);
                //         dbUser.LastActivityAt = now;
                //         dbUser.LastLoginAt = now;
                //         await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                //         ClearCache(dbUser.EmailNormalized, dbUser.ApiKey, dbUser.Id, dbUser.UserNameNormalized);
                //     }
                // });
                result = true;
            }
        }

        return new ResponseModel<ApiResponse>
        {
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
