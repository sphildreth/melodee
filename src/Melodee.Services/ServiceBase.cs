using EasyCaching.Core;
using Melodee.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Melodee.Services;

public abstract class ServiceBase (
    ILogger<SettingService> logger,
    IEasyCachingProviderFactory cachingProviderFactory,    
    IDbContextFactory<MelodeeDbContext> contextFactory)
{
    protected ILogger<SettingService> Logger { get; } = logger;
    protected IEasyCachingProviderFactory CachingProviderFactory { get; } = cachingProviderFactory;
    protected IDbContextFactory<MelodeeDbContext> ContextFactory { get; } = contextFactory; 
}
