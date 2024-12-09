using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Validation;
using Melodee.Services;
using Melodee.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

/// <summary>
/// Housekeeping for Artist Images
/// </summary>
public class ArtistImageJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ArtistService artistService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistImageSearchEngineService imageSearchEngine,
    IHttpClientFactory httpClientFactory,
    IImageValidator imageValidator) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        var httpClient = httpClientFactory.CreateClient();
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken))
        {
            var readyToProcessStatus = SafeParser.ToNumber<int>(MetaDataModelStatus.ReadyToProcess);
            var artists = await scopedContext.Artists
                .Include(x => x.Library)
                .Include(x => x.Albums)
                .Where(x => !x.Library.IsLocked && !x.IsLocked && x.ImageCount == 0 && x.MetaDataStatus == readyToProcessStatus)
                .ToArrayAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var maxNumberOfImagesAllowed = configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfArtistImages);
            if (maxNumberOfImagesAllowed == 0)
            {
                maxNumberOfImagesAllowed = short.MaxValue;
            }
            foreach (var artist in artists)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var artistFileDirectory = artist.ToFileSystemDirectoryInfo();
                var imageCount = artistFileDirectory.FileInfosForExtension("jpg").Count();
                if (imageCount == 0)
                {
                    var albumImageSearchResult = await imageSearchEngine.DoSearchAsync(
                            artist.ToArtistQuery(artist.Albums.Select(x => x.ToKeyValue()).ToArray()),
                            1,
                            context.CancellationToken)
                        .ConfigureAwait(false);
                    if (albumImageSearchResult.IsSuccess)
                    {
                        var imageFileName = artistFileDirectory.GetNextFileNameForType(maxNumberOfImagesAllowed, Common.Data.Models.Artist.ImageType).Item1;
                        if (await httpClient.DownloadFileAsync(
                                albumImageSearchResult.Data.First().MediaUrl,
                                imageFileName,
                                async (_, newFileInfo, _) => (await imageValidator.ValidateImage(newFileInfo, context.CancellationToken)).Data.IsValid,
                                context.CancellationToken))
                        {
                            artist.LastUpdatedAt = now;
                            artist.ImageCount = 1;
                            artist.MetaDataStatus = SafeParser.ToNumber<int>(MetaDataModelStatus.UpdatedImages);
                            artistService.ClearCache(artist);
                        }

                    }
                }

            }
            
        }
    }
}
