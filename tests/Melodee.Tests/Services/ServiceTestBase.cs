using System.Data.Common;
using System.Net;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Serialization;
using Melodee.Plugins.Scrobbling;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Quartz;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Core;

namespace Melodee.Tests.Services;

public class ConsoleLogSink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;

    public ConsoleLogSink(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        if (logEvent.Exception != null)
        {
            message += $"\n{logEvent.Exception}";
        }
        Console.WriteLine(DateTimeOffset.Now.ToString() + " "  + message);
    }
}

public static class ConsoleLogSinkExtensions
{
    public static LoggerConfiguration ConsoleLogSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new ConsoleLogSink(formatProvider));
    }
}

public abstract class ServiceTestBase : IDisposable, IAsyncDisposable
{
    private readonly DbConnection _dbConnection;

    private readonly DbContextOptions<MelodeeDbContext> _dbContextOptions;

    protected ServiceTestBase()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.ConsoleLogSink()
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
            username,
            "1.16.1",
            "json",
            null,
            null,
            password,
            salt,
            new UserPlayer(null,
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
            new DefaultImages
            {
                AlbumCoverBytes = [],
                ArtistBytes = [],
                UserAvatarBytes = []
            },
            MockSettingService(),
            GetUserService(),
            GetArtistService(),
            GetAlbumService(),
            GetSongService(),
            new AlbumDiscoveryService(
                Logger, 
                CacheManager, 
                MockFactory(), 
                MockSettingService(), 
                Serializer),
            new Mock<IScheduler>().Object,
            GetScrobbleService(),
            GetLibraryService());
    }

    protected ArtistService GetArtistService()
    {
        return new ArtistService(Logger, CacheManager, MockFactory());
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
            MockSettingService(),
            Serializer
        );
    }
    
    protected ScrobbleService GetScrobbleService()
    {
        return new ScrobbleService(
            Logger,
            CacheManager,
            MockFactory(),
            MockSettingService(),
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
        
        var clientHandlerStub = new HttpHandlerStubDelegate((request, cancellationToken) => {
            var response = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            return Task.FromResult(response);
        });
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(m => m.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(clientHandlerStub));      
        return factoryMock.Object;
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

    protected IMelodeeConfigurationFactory MockConfigurationFactory()
    {
        var mock = new Mock<IMelodeeConfigurationFactory>();
        mock.Setup(f => f.GetConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.NewPluginsConfiguration);
        return mock.Object;
    }

    protected ISettingService MockSettingService()
    {
       var mock = new Mock<ISettingService>();
       mock.Setup(f => f.GetMelodeeConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TestsBase.NewPluginsConfiguration);
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
