using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Utility;
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
            MockConfigurationFactory(),
            MockLibraryService(),
            Serializer,
            MockFactory(),
            GetArtistService(),
            GetAlbumService(),
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
                    Serializer, 
                    MockHttpClientFactory()), 
                GetArtistSearchEngineService(),
                MockHttpClientFactory()));
        
        var mockJobExecutionContext = new Mock<IJobExecutionContext>();
        mockJobExecutionContext.Setup(f => f.CancellationToken).Returns(CancellationToken.None);
        mockJobExecutionContext.Setup(f => f.JobDetail).Returns(new JobDetailImpl(nameof(LibraryProcessJob), typeof(LibraryProcessJob)));

        var startMessageReceived = false;
        var endMessageReceived = false;
        job.OnProcessingEvent += (sender, args) =>
        {
            switch (args.Type)
            {
                case ProcessingEventType.Start:
                    startMessageReceived = true;
                    break;
                
                case ProcessingEventType.Stop:
                    endMessageReceived = true;
                    break;
            }
            
        };
        await job.Execute(mockJobExecutionContext.Object);
        
        var listResult = await GetArtistService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.NotEmpty(listResult.Data);
        Assert.True(listResult.TotalPages > 0);
        Assert.True(listResult.TotalCount > 0);

        Assert.True(startMessageReceived);
        Assert.True(endMessageReceived);

    }
}
