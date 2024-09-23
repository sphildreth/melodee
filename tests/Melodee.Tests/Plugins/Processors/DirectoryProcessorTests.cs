using Melodee.Common.Models;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Serilog;

namespace Melodee.Tests.Plugins.Processors;

public class DirectoryProcessorTests
{
    // 

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
            var validator = new ReleaseValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config), 
                new NullScript(config), 
                validator, 
                new ReleaseEditProcessor(config, 
                    new ReleasesDiscoverer(validator, config), 
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
    public async Task ValidateDirectoryGetProcessedIsNotSuccess()
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
            var validator = new ReleaseValidator(config);
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config),
                new NullScript(config),
                validator,
                new ReleaseEditProcessor(config,
                    new ReleasesDiscoverer(validator, config),
                    new AtlMetaTag(new MetaTagsProcessor(config), config),
                    validator), config);  
            var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);

        }
    }
}
