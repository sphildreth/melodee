using EasyCaching.Core;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Melodee.Services;

public sealed class SettingService(
    ILogger<SettingService> logger, 
    IEasyCachingProviderFactory cachingProviderFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cachingProviderFactory, contextFactory)
{
    
    public Task<OperationResult<T?>> GetAsync<T>(string key)
    {
        throw new NotImplementedException();
    }
}
