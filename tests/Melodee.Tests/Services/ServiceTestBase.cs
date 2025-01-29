using System.Data.Common;
using System.Net;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Quartz;
using Rebus.Bus;
using Serilog;
using ServiceStack.Data;

namespace Melodee.Tests.Services;

public abstract class ServiceTestBase : IDisposable, IAsyncDisposable
{
    private readonly DbConnection _dbConnection;

    private readonly DbContextOptions<MelodeeDbContext> _dbContextOptions;

    protected ServiceTestBase()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
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
        return new ApiRequest(
            [],
            false,
            username,
            "1.16.1",
            "json",
            null,
            null,
            password,
            salt,
            null,
            null,
            new UserPlayer(null,
                null,
                null,
                null));
    }

    protected IDbConnectionFactory MockDbContextFactory()
    {
        var mockFactory = new Mock<IDbConnectionFactory>();
        return mockFactory.Object;
    }

    protected IMusicBrainzRepository GetMusicBrainzRepository()
    {
        return new SQLiteMusicBrainzRepository(Log.Logger,
            MockConfigurationFactory(),
            MockDbContextFactory());
    }

    // protected IEventPublisher<UserLoginEvent> MockUserEventPublisher()
    // {
    //     var mockEventPublisher = new Mock<IEventPublisher<UserLoginEvent>>();
    //     return mockEventPublisher.Object;
    // }

    // protected IEventPublisher<AlbumUpdatedEvent> MockAlbumUpdatedEventPublisher()
    // {
    //     var mockEventPublisher = new Mock<IEventPublisher<AlbumUpdatedEvent>>();
    //     return mockEventPublisher.Object;
    // }

    protected ArtistSearchEngineService GetArtistSearchEngineService()
    {
        return new ArtistSearchEngineService(Logger,
            CacheManager,
            MockSettingService(),
            MockConfigurationFactory(),
            MockFactory(),
            GetMusicBrainzRepository());
    }

    protected ImageConvertor GetImageConvertor()
    {
        return new ImageConvertor(TestsBase.NewPluginsConfiguration());
    }

    protected IImageValidator GetImageValidator()
    {
        return new ImageValidator(TestsBase.NewPluginsConfiguration());
    }

    protected IAlbumValidator GetAlbumValidator()
    {
        return new AlbumValidator(TestsBase.NewPluginsConfiguration());
    }

    protected AlbumImageSearchEngineService GetAlbumImageSearchEngineService()
    {
        return new AlbumImageSearchEngineService(Logger,
            CacheManager,
            Serializer,
            MockSettingService(),
            MockConfigurationFactory(),
            MockFactory(),
            GetMusicBrainzRepository(),
            MockHttpClientFactory());
    }

    protected OpenSubsonicApiService GetOpenSubsonicApiService()
    {
        return new OpenSubsonicApiService(
            Logger,
            CacheManager,
            MockFactory(),
            new DefaultImages
            {
                AlbumCoverBytes = [],
                ArtistBytes = [],
                UserAvatarBytes = []
            },
            MockConfigurationFactory(),
            GetUserService(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService(),
            new AlbumDiscoveryService(
                Logger,
                CacheManager,
                MockFactory(),
                MockConfigurationFactory(),
                Serializer),
            new Mock<IScheduler>().Object,
            GetScrobbleService(),
            GetLibraryService(),
            GetArtistSearchEngineService(),
            MockBus());
    }

    // protected InMemoryEventBusPublisher<UserLoginEvent> MockUserLoginEventBusPublisher()
    // {
    //     var mockFactory = new Mock<InMemoryEventBusPublisher<UserLoginEvent>>();
    //     return mockFactory.Object;
    // }

    protected ArtistService GetArtistService()
    {
        return new ArtistService(Logger, CacheManager, MockConfigurationFactory(), MockFactory(), Serializer, MockHttpClientFactory());
    }

    protected AlbumService GetAlbumService()
    {
        return new AlbumService(Logger, CacheManager, MockFactory());
    }

    protected SongService GetSongService()
    {
        return new SongService(Logger, CacheManager, MockFactory());
    }

    protected INowPlayingRepository GetNowPlayingRepository()
    {
        return new NowPlayingInMemoryRepository();
    }

    protected LibraryService GetLibraryService()
    {
        return new LibraryService
        (
            Logger,
            CacheManager,
            MockFactory(),
            MockConfigurationFactory(),
            Serializer,
            MockBus()
        );
    }

    protected ScrobbleService GetScrobbleService()
    {
        return new ScrobbleService(
            Logger,
            CacheManager,
            MockFactory(),
            MockConfigurationFactory(),
            GetNowPlayingRepository(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService());
    }

    protected IHttpClientFactory MockHttpClientFactory()
    {
        // var clientHandlerMock = new Mock<HttpHandlerStubDelegate>();
        // clientHandlerMock.Protected()
        //     .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        //     .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
        //     .Verifiable();
        // clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());
        //
        // var httpClient = new HttpClient(clientHandlerMock.Object);

        // var clientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        // clientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();
        //
        // clientFactoryMock.Verify(cf => cf.CreateClient());
        // clientHandlerMock.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        // return clientFactoryMock.Object;

        var clientHandlerStub = new HttpHandlerStubDelegate((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return Task.FromResult(response);
        });
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(clientHandlerStub));
        return factoryMock.Object;
    }

    protected LibraryService MockLibraryService()
    {
        var mock = new Mock<LibraryService>();
        // mock.Setup(f
        //     => f.ListAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.TestLibraries());
        mock.Setup(f => f.ListAsync(
                It.Is<PagedRequest>(_ => true),
                It.Is<CancellationToken>(_ => true)))
            .ReturnsAsync(TestsBase.TestLibraries());
        mock.Setup(f
            => f.GetStorageLibrariesAsync(It.Is<CancellationToken>(_ => true))).ReturnsAsync(new OperationResult<Library[]>
        {
            Data = TestsBase.TestLibraries().Data.Where(x => x.TypeValue == LibraryType.Storage).ToArray()
        });
        mock.Setup(f
            => f.GetStagingLibraryAsync(It.Is<CancellationToken>(_ => true))).ReturnsAsync(TestsBase.TestStagingLibrary());
        return mock.Object;
    }

    protected UserService GetUserService()
    {
        return new UserService(
            Logger,
            CacheManager,
            MockFactory(),
            MockConfigurationFactory(),
            GetLibraryService(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService(),
            MockBus());
    }

    protected IBus MockBus()
    {
        var busMock = new Mock<IBus>();
        busMock.Setup(b => b.SendLocal(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.CompletedTask);
        return busMock.Object;
    }

    protected IMelodeeConfigurationFactory MockConfigurationFactory()
    {
        var mock = new Mock<IMelodeeConfigurationFactory>();
        mock.Setup(f => f.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.NewPluginsConfiguration);
        return mock.Object;
    }

    protected SettingService MockSettingService()
    {
        var mock = new Mock<SettingService>();
        mock.Setup(f => f.GetAllSettingsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.NewConfiguration());
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
