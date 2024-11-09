using Melodee.Common.Data;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.Conversion.Image;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class ImageConversionService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<MelodeeModels.OperationResult<bool>> ConvertImageAsync(FileInfo imageFileInfo, CancellationToken cancellationToken = default)
    {
        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
        var imageConvertor = new ImageConvertor(configuration);
        var convertResult = await imageConvertor.ProcessFileAsync(imageFileInfo.ToDirectorySystemInfo(), imageFileInfo.ToFileSystemInfo(), cancellationToken);
        return new MelodeeModels.OperationResult<bool>
        {
            Data = convertResult.IsSuccess
        };
    }
}
