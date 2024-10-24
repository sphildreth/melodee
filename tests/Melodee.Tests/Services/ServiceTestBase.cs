using System.Data.Common;
using Melodee.Common.Data;
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
    
    protected ILogger Logger { get; }
    
    protected Serializer Serializer { get; set; }
    
    protected ICacheManager CacheManager { get; }
    
    public ServiceTestBase()
    {
        Logger = new Mock<ILogger>().Object;
        Serializer = new Serializer(Logger);
        CacheManager = new FakeCacheManager(Logger, TimeSpan.FromDays(1), Serializer);
        
        _dbConnection = new SqliteConnection("Filename=:memory:");
        _dbConnection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseSqlite(_dbConnection, x => x.UseNodaTime())
            .Options;

        using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }

    protected IDbContextFactory<MelodeeDbContext> MockFactory()
    {
        var mockFactory = new Mock<IDbContextFactory<MelodeeDbContext>>();
        mockFactory.Setup(f 
            => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => new MelodeeDbContext(_dbContextOptions));
        return mockFactory.Object;        
    }

    protected UserService GetUserService()
        => new UserService(Logger, CacheManager, MockFactory());

    public void Dispose()
    {
        _dbConnection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
    }
}
