using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class ImageConversionService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory
)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<MelodeeModels.OperationResult<bool>> ConvertImageAsync(FileInfo imageFileInfo,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var imageConvertor = new ImageConvertor(configuration);
        var convertResult = await imageConvertor.ProcessFileAsync(imageFileInfo.ToDirectorySystemInfo(),
            imageFileInfo.ToFileSystemInfo(), cancellationToken);
        return new MelodeeModels.OperationResult<bool>
        {
            Data = convertResult.IsSuccess
        };
    }
}
