using Melodee.Common.Models;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;

namespace Melodee.Tests.Plugins.Processors;

public class DirectoryProcessorTests
{
    [Fact]
    public async Task ValidateDirectoryGetProcessed()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/2024-06-14";
        var dirInfo = new System.IO.DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var processor = new DirectoryProcessor(
                new PreDiscoveryScript(config), 
                new NullScript(config), 
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