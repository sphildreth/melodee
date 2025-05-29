using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Blazor.Services;

public class StartupMelodeeConfigurationService(
    Serilog.ILogger logger,
    IDbContextFactory<MelodeeDbContext> contextFactory, 
    LibraryService libraryService,
    IConfiguration configuration)
    : IStartupMelodeeConfigurationService
{
    public async Task UpdateConfigurationFromEnvironmentAsync(CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var storagePath = configuration.GetValue<string>("MELODEE_STORAGE_PATH");
            if (storagePath.Nullify() != null)
            {
                var storageLibrary = await libraryService.GetStorageLibrariesAsync(cancellationToken).ConfigureAwait(false);
                if (storageLibrary.IsSuccess)
                {
                    storageLibrary.Data.First().Path = storagePath!;
                    var result = await libraryService.UpdateAsync(storageLibrary.Data.First(), cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        logger.Information("Storage path updated to {Path}", storagePath);   
                    }
                }
            }
            var inboundPath = configuration.GetValue<string>("MELODEE_INBOUND_PATH");
            if (inboundPath.Nullify() != null)
            {
                var inboundLibrary = await libraryService.GetInboundLibraryAsync(cancellationToken).ConfigureAwait(false);
                if (inboundLibrary.IsSuccess)
                {
                    inboundLibrary.Data.Path = inboundPath!;
                    var result = await libraryService.UpdateAsync(inboundLibrary.Data, cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        logger.Information("Inbound path updated to {Path}", inboundPath);   
                    }
                }
            }            
            var stagingPath = configuration.GetValue<string>("MELODEE_STAGING_PATH");
            if (stagingPath.Nullify() != null)
            {
                var stagingLibrary = await libraryService.GetStagingLibraryAsync(cancellationToken).ConfigureAwait(false);
                if (stagingLibrary.IsSuccess)
                {
                    stagingLibrary.Data.Path = stagingPath!;
                    var result = await libraryService.UpdateAsync(stagingLibrary.Data, cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        logger.Information("Staging path updated to {Path}", stagingPath);   
                    }
                }
            }              
            var userImagePath = configuration.GetValue<string>("MELODEE_USER_IMAGES_PATH");
            if (userImagePath.Nullify() != null)
            {
                var userImageLibrary = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);
                if (userImageLibrary.IsSuccess)
                {
                    userImageLibrary.Data.Path = userImagePath!;
                    var result = await libraryService.UpdateAsync(userImageLibrary.Data, cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        logger.Information("User image path updated to {Path}", userImagePath);   
                    }
                }
            }               
            var playlistPath = configuration.GetValue<string>("MELODEE_PLAYLISTS_PATH");
            if (playlistPath.Nullify() != null)
            {
                var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
                if (playlistLibrary.IsSuccess)
                {
                    playlistLibrary.Data.Path = playlistPath!;
                    var result = await libraryService.UpdateAsync(playlistLibrary.Data, cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        logger.Information("Inbound path updated to {Path}", inboundPath);   
                    }
                }
            }            
            await scopedContext.SaveChangesAsync(cancellationToken);
        }
    }
    

}
