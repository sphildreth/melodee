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
    public const string CacheName = "melodee";
    protected static TimeSpan DefaultCacheDuration = TimeSpan.FromDays(1);
    
    protected ILogger<SettingService> Logger { get; } = logger;
    protected IEasyCachingProviderFactory CachingProviderFactory { get; } = cachingProviderFactory;
    protected IDbContextFactory<MelodeeDbContext> ContextFactory { get; } = contextFactory; 
}
