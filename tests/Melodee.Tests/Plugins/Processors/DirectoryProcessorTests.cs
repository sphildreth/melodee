using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.Discovery.Albums;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Serilog;

namespace Melodee.Tests.Plugins.Processors;

public class DirectoryProcessorTests
{
    [Fact]
    public async Task ValidateDirectoryGetProcessedIsSuccess()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/home/steven/incoming/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var testFile = @"/home/steven/incoming/melodee_test/inbound/The Sound Of Melodic Techno Vol. 21/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var validator = new AlbumValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config), 
                new NullScript(config), 
                validator, 
                new AlbumEditProcessor(config, 
                    new AlbumsDiscoverer(validator, config), 
                    new AtlMetaTag(new MetaTagsProcessor(config), config),
                    validator),
                config);
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
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
            .WriteTo.File("/home/steven/incoming/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var testFile = @"/home/steven/incoming/melodee_test/inbound/Pixel_-_Reality_Strikes_Back-2004-MYCEL/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var validator = new AlbumValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config),
                new NullScript(config),
                validator,
                new AlbumEditProcessor(config,
                    new AlbumsDiscoverer(validator, config),
                    new AtlMetaTag(new MetaTagsProcessor(config), config),
                    validator), config);  
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

        }
    }
    
    [Fact]
    public async Task ValidateDirectoryGetProcessedSongsWithMultipleArtistsIsSuccess()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/home/steven/incoming/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var testFile = @"/home/steven/incoming/melodee_test/tests/Songs_with_artists_2/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var validator = new AlbumValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config),
                new NullScript(config),
                validator,
                new AlbumEditProcessor(config,
                    new AlbumsDiscoverer(validator, config),
                    new AtlMetaTag(new MetaTagsProcessor(config), config),
                    validator), config);  
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data.NumberOfAlbumFilesProcessed);
        }
    }    
    
    [Fact]
    public async Task ValidateAllAlbumsForDirectoryAsyncShouldBeSingleAlbumSongsWithManyArtists()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/home/steven/incoming/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var testFile = @"/home/steven/incoming/melodee_test/tests/Songs_with_artists/";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            foreach (var file in dirInfo.EnumerateFiles("*.melodee.json"))
            {
                file.Delete();
            }
            var config = TestsBase.NewConfiguration;
            var validator = new AlbumValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config),
                new NullScript(config),
                validator,
                new AlbumEditProcessor(config,
                    new AlbumsDiscoverer(validator, config),
                    new AtlMetaTag(new MetaTagsProcessor(config), config),
                    validator), config);

            var allAlbums = await processor.AllAlbumsForDirectoryAsync(dirInfo.ToDirectorySystemInfo());
            Assert.NotNull(allAlbums);
            Assert.True(allAlbums.IsSuccess);   
            Assert.Single(allAlbums.Data.Item1);
        }
    }    
    
}
