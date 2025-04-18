using Melodee.Common.Models;
using Melodee.Common.Services.Scanning;
using Serilog;

namespace Melodee.Tests.Services;

public class DirectoryProcessorToStagingServiceTests : ServiceTestBase
{
    private DirectoryProcessorToStagingService CreateDirectoryProcessorService()
    {
        return new DirectoryProcessorToStagingService(
            Log.Logger,
            CacheManager,
            MockFactory(),
            MockConfigurationFactory(),
            MockLibraryService(),
            Serializer,
            GetMediaEditService(),
            GetArtistSearchEngineService(),
            GetAlbumImageSearchEngineService(),
            MockHttpClientFactory()
        );
    }

    [Fact]
    public async Task ValidateDirectoryGetProcessedIsSuccess()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var testFile = @"/melodee_test/inbound/The Sound Of Melodic Techno Vol. 21/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var processor = CreateDirectoryProcessorService();
            await processor.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            }, null, null);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }
    }

    [Fact]
    public async Task ValidateDirectoryGetProcessedIsSuccess2()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var testFile = @"/melodee_test/inbound/Pixel_-_Reality_Strikes_Back-2004-MYCEL/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var processor = CreateDirectoryProcessorService();
            await processor.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            }, null, null);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }
    }

    // [Fact]
    // public async Task ValidateDirectoryGetAlbumsWithMultipleReleasesSameFolder()
    // {
    //     Log.Logger = new LoggerConfiguration()
    //         .MinimumLevel.Verbose()
    //         .WriteTo.Console()
    //         .WriteTo.File("/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
    //         .CreateLogger();
    //
    //     var testFile = @"/melodee_test/inbound/3AlbumsMixed/";
    //     var dirInfo = new DirectoryInfo(testFile);
    //     if (dirInfo.Exists)
    //     {
    //         foreach (var file in dirInfo.EnumerateFiles("*.melodee.json"))
    //         {
    //             file.Delete();
    //         }
    //
    //         var config = TestsBase.NewPluginsConfiguration();
    //         var processor = CreateDirectoryProcessorService();
    //         await processor.InitializeAsync(TestsBase.NewPluginsConfiguration());
    //         ISongPlugin[] songPlugins =
    //         [
    //             new AtlMetaTag(new MetaTagsProcessor(config, Serializer), GetImageConvertor(), GetImageValidator(), config)
    //         ];            
    //         var allAlbums = await processor.AllAlbumsForDirectoryAsync(
    //             dirInfo.ToDirectorySystemInfo(),
    //             GetAlbumValidator(),
    //             songPlugins.ToArray(),
    //             config
    //         );
    //         Assert.NotNull(allAlbums);
    //         Assert.True(allAlbums.IsSuccess);
    //         Assert.Equal(3, allAlbums.Data.Item1.Count());
    //
    //         // Ensure the three albums found in the folder don't have the same songs
    //         var firstAlbumSongCount = allAlbums.Data.Item1.First().Songs?.Count() ?? 0;
    //         var secondAlbumSongCount = allAlbums.Data.Item1.Skip(1).Take(1).First().Songs?.Count() ?? 0;
    //         var thirdAlbumSongCount = allAlbums.Data.Item1.Last().Songs?.Count() ?? 0;
    //         Assert.True(firstAlbumSongCount != secondAlbumSongCount && firstAlbumSongCount != thirdAlbumSongCount);
    //     }
    // }

    [Fact]
    public async Task ValidateDirectoryGetProcessedSongsWithMultipleArtistsIsSuccess()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var testFile = @"/melodee_test/tests/Songs_with_artists_2/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewPluginsConfiguration();
            var processor = CreateDirectoryProcessorService();
            await processor.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            }, null, null);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data.NumberOfAlbumFilesProcessed);
        }
    }

    //[Fact]
    // public async Task ValidateAllAlbumsForDirectoryAsyncShouldBeSingleAlbumSongsWithManyArtists()
    // {
    //     Log.Logger = new LoggerConfiguration()
    //         .MinimumLevel.Verbose()
    //         .WriteTo.Console()
    //         .WriteTo.File("/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
    //         .CreateLogger();
    //
    //     var testFile = @"/melodee_test/tests/Songs_with_artists/";
    //     var dirInfo = new DirectoryInfo(testFile);
    //     if (dirInfo.Exists)
    //     {
    //         foreach (var file in dirInfo.EnumerateFiles("*.melodee.json"))
    //         {
    //             file.Delete();
    //         }
    //         var config = TestsBase.NewPluginsConfiguration();
    //         ISongPlugin[] songPlugins =
    //         [
    //             new AtlMetaTag(new MetaTagsProcessor(config, Serializer),GetImageConvertor(),GetImageValidator(), config)
    //         ]; 
    //         var processor = CreateDirectoryProcessorService();
    //         await processor.InitializeAsync(TestsBase.NewPluginsConfiguration());
    //         var allAlbums = await processor.AllAlbumsForDirectoryAsync(
    //             dirInfo.ToDirectorySystemInfo(),
    //             GetAlbumValidator(),
    //             songPlugins.ToArray(),
    //             config
    //         );
    //         Assert.NotNull(allAlbums);
    //         Assert.True(allAlbums.IsSuccess);
    //         Assert.Single(allAlbums.Data.Item1);
    //     }
    // }
}
