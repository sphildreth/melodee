using Melodee.Common.Models;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Serilog;

namespace Melodee.Tests.Plugins.Processors;

public class DirectoryProcessorTests
{
    [Fact]
    public async Task ValidateDirectoryGetProcessed()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("/home/steven/incoming/melodee_test/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var testFile = @"/home/steven/incoming/melodee_test/inbound/Pixel_-_Reality_Strikes_Back-2004-MYCEL/";
        var dirInfo = new System.IO.DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config), 
                new NullScript(config), 
                new ReleaseValidator(config), 
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
}
