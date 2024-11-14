using Melodee.Jobs;
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
        var job = new LibraryProcessJob(Logger,
            GetSettingService(),
            GetLibraryService(),
            Serializer,
            MockFactory(),
            GetArtistService(),
            new AlbumDiscoveryService(Logger,
                CacheManager,
                MockFactory(),
                GetSettingService(),
                Serializer));
        
        var mockJobExecutionContext = new Mock<IJobExecutionContext>();
        // TODO add CancellationToken
        await job.Execute(mockJobExecutionContext.Object);

    }
}
