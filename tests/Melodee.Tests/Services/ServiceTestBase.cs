using System.Data.Common;
using Dorssel.EntityFrameworkCore;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;

namespace Melodee.Tests.Services;

public abstract class ServiceTestBase : IDisposable, IAsyncDisposable
{
    private readonly DbConnection _dbConnection;

    private readonly DbContextOptions<MelodeeDbContext> _dbContextOptions;

    protected ServiceTestBase()
    {
        Logger = new Mock<ILogger>().Object;
        Serializer = new Serializer(Logger);
        CacheManager = new FakeCacheManager(Logger, TimeSpan.FromDays(1), Serializer);

        _dbConnection = new SqliteConnection("Filename=:memory:");
        _dbConnection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseSqlite(_dbConnection, x => x.UseNodaTime())
            .UseSqliteTimestamp()
            .Options;

        using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }

    private ILogger Logger { get; }

    private Serializer Serializer { get; set; }

    private ICacheManager CacheManager { get; }

    public async ValueTask DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
    }

    public void Dispose()
    {
        _dbConnection.Dispose();
    }

    protected IDbContextFactory<MelodeeDbContext> MockFactory()
    {
        var mockFactory = new Mock<IDbContextFactory<MelodeeDbContext>>();
        mockFactory.Setup(f
            => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => new MelodeeDbContext(_dbContextOptions));
        return mockFactory.Object;
    }

    protected UserService GetUserService()
    {
        return new UserService(Logger, CacheManager, MockFactory());
    }

    protected SettingService GetSettingService()
    {
        return new SettingService(Logger, CacheManager, MockFactory());
    }

    protected static void AssertResultIsSuccessful<T>(PagedResult<T> result) where T : notnull
    {
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);        
    }    
    
    protected static void AssertResultIsSuccessful<T>(OperationResult<T?>? result)
    {
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);        
    }
}
