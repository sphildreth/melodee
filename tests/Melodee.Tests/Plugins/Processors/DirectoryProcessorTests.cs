using Melodee.Plugins.Processor;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Tests.Plugins.Processors;

public class DirectoryProcessorTests
{
    [Fact]
    public async Task ValidateDirectoryGetProcessed()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var dirInfo = new System.IO.DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            var processor = new DirectoryProcessor(config);
            var result = await processor.ProcessDirectoryAsync(new DirectoryInfo
            {
                Path = dirInfo.FullName,
                ShortName = dirInfo.Name
            });
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

        }
    }
}