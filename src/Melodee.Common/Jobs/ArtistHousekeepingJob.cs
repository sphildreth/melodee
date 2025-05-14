using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Services;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Housekeeping for Artist
/// </summary>
public class ArtistHousekeepingJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ArtistService artistService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistImageSearchEngineService imageSearchEngine,
    IHttpClientFactory httpClientFactory) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken)
            .ConfigureAwait(false);
        var batchSize = configuration.GetValue<int>(SettingRegistry.DefaultsBatchSize);
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var httpClient = httpClientFactory.CreateClient();
        var imageValidator = new ImageValidator(configuration);
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken))
        {
            var readyToProcessStatus = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);
            var artists = await scopedContext.Artists
                .Include(x => x.Library)
                .Include(x => x.Albums)
                .Where(x => !x.Library.IsLocked && !x.IsLocked && (x.ImageCount == null || x.ImageCount == 0) &&
                            x.MetaDataStatus == readyToProcessStatus)
                .Take(batchSize)
                .ToArrayAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (artists.Length == 0)
            {
                return;
            }

            var maxNumberOfImagesAllowed =
                configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfArtistImages);
            if (maxNumberOfImagesAllowed == 0)
            {
                maxNumberOfImagesAllowed = short.MaxValue;
            }

            Logger.Debug("[{JobName}] found [{Count}] artists without images.", nameof(ArtistHousekeepingJob),
                artists.Length);

            var updatedArtists = new List<Artist>();

            foreach (var artist in artists)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var artistFileDirectory = artist.ToFileSystemDirectoryInfo();
                var albumImageSearchResult = await imageSearchEngine.DoSearchAsync(
                        artist.ToArtistQuery(artist.Albums.Select(x => x.ToKeyValue()).ToArray()),
                        1,
                        context.CancellationToken)
                    .ConfigureAwait(false);
                if (!albumImageSearchResult.IsSuccess || albumImageSearchResult.Data.Length == 0)
                {
                    continue;
                }

                var imageFileName = artistFileDirectory.GetNextFileNameForType(Artist.ImageType).Item1;
                if (await httpClient.DownloadFileAsync(
                        albumImageSearchResult.Data.First().MediaUrl,
                        imageFileName,
                        async (_, newFileInfo, _) =>
                            (await imageValidator
                                .ValidateImage(newFileInfo, PictureIdentifier.Artist, context.CancellationToken)
                                .ConfigureAwait(false)).Data.IsValid,
                        context.CancellationToken).ConfigureAwait(false))
                {
                    artist.LastUpdatedAt = now;
                    artist.ImageCount = 1;
                    artist.MetaDataStatus = SafeParser.ToNumber<int>(MetaDataModelStatus.UpdatedImages);
                    await artistService.ClearCacheAsync(artist, context.CancellationToken);
                    updatedArtists.Add(artist);
                    Logger.Information("[{JobName}] Updated artist image for artist [{ArtistName}]",
                        nameof(ArtistHousekeepingJob), artist.Name);
                }
            }

            if (updatedArtists.Count > 0)
            {
                await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            }
        }
    }
}
