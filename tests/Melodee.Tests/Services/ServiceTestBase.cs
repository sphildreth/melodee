using System.Data.Common;
using System.Net;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Quartz;
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

        _dbConnection = new SqliteConnection("Filename=:memory:;Cache=Shared;");
        _dbConnection.Open();

        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new TimeSpanHandler());
        SqlMapper.AddTypeHandler(new InstantHandler());

        _dbContextOptions = new DbContextOptionsBuilder<MelodeeDbContext>()
            .UseSqlite(_dbConnection, x => x.UseNodaTime())
            .Options;

        using (var context = new MelodeeDbContext(_dbContextOptions))
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }

    protected ILogger Logger { get; }

    protected Serializer Serializer { get; set; }

    protected ICacheManager CacheManager { get; }

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

    protected ApiRequest GetApiRequest(string username, string salt, string password)
    {
        return new ApiRequest(username,
            "1.16.1",
            "json",
            null,
            null,
            password,
            salt,
            new ApiRequestPlayer(null,
                null,
                null,
                null));
    }

    protected OpenSubsonicApiService GetOpenSubsonicApiService()
    {
        return new OpenSubsonicApiService(
            Logger,
            CacheManager,
            MockFactory(),
            MockSettingService(),
            GetUserService(),
            GetArtistService(),
            GetAlbumService(),
            new Mock<IScheduler>().Object);
    }

    protected ArtistService GetArtistService()
    {
        return new ArtistService(Logger, CacheManager, MockFactory());
    }

    protected AlbumService GetAlbumService()
    {
        return new AlbumService(Logger, CacheManager, MockFactory());
    }

    protected IHttpClientFactory MockHttpClientFactory()
    {
        var clientHandlerMock = new Mock<DelegatingHandler>();
        clientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();
        clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());

        var httpClient = new HttpClient(clientHandlerMock.Object);

        var clientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        clientFactoryMock.Setup(cf => cf.CreateClient()).Returns(httpClient).Verifiable();

        clientFactoryMock.Verify(cf => cf.CreateClient());
        clientHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        return clientFactoryMock.Object;
    }

    protected ILibraryService MockLibraryService()
    {
        var mock = new Mock<ILibraryService>();
        mock.Setup(f
            => f.ListAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.TestLibraries());
        mock.Setup(f
            => f.GetLibraryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.TestLibrary());
        mock.Setup(f
            => f.GetStagingLibraryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.TestStagingLibrary());         
        return mock.Object;        
    }

    protected UserService GetUserService()
    {
        return new UserService(Logger, CacheManager, MockFactory(), MockSettingService());
    }

    protected ISettingService MockSettingService()
    {
       var mock = new Mock<ISettingService>();
       mock.Setup(f
           => f.GetMelodeeConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.NewPluginsConfiguration);
       return mock.Object;
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
