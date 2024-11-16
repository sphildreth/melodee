using Melodee.Common.Constants;
using Melodee.Jobs;
using Melodee.Services;
using Melodee.Services.Scanning;
using Melodee.Tests.Services;
using Moq;
using Quartz;
using Quartz.Impl;

namespace Melodee.Tests.Jobs;

public class LibraryProcessJobTests : ServiceTestBase
{
    [Fact]
    public async Task ExecuteJob()
    {
        var albumDiscoveryService = new AlbumDiscoveryService(Logger,
            CacheManager,
            MockFactory(),
            MockSettingService(),
            Serializer);
        
        var job = new LibraryProcessJob(Logger,
            MockSettingService(),
            MockLibraryService(),
            Serializer,
            MockFactory(),
            GetArtistService(),
            albumDiscoveryService,
            new DirectoryProcessorService(Logger,
                CacheManager,
                MockFactory(),
                MockSettingService(),
                MockLibraryService(),
                Serializer,
                new MediaEditService(Logger,
                    CacheManager,
                    MockFactory(),
                    MockSettingService(),
                    MockLibraryService(),
                    albumDiscoveryService,
                    Serializer, MockHttpClientFactory())));
        
        var mockJobExecutionContext = new Mock<IJobExecutionContext>();
        mockJobExecutionContext.Setup(f => f.CancellationToken).Returns(CancellationToken.None);
        mockJobExecutionContext.Setup(f => f.JobDetail).Returns(new JobDetailImpl(nameof(LibraryProcessJob), typeof(LibraryProcessJob)));
        await job.Execute(mockJobExecutionContext.Object);

    }
}
