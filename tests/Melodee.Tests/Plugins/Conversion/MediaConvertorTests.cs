using Melodee.Common.Extensions;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins;

namespace Melodee.Tests.Plugins.Conversion;

public class MediaConvertorTests
{
    [Fact] public async Task ValidateConvertingFlacToMp3Async()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testflac.flac";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Media.MediaConvertor(TestsBase.NewConfiguration);
            var convertorResult = await convertor.ProcessFileAsync(fileInfo.ToFileSystemInfo());
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new System.IO.FileInfo(convertorResult.Data.FullName());
            Assert.True(convertedFileInfo.Exists);

        }
    }       
    
    [Fact]
    public async Task ValidateConvertingNonMediaFailsAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testbmp.bmp";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Media.MediaConvertor(new Configuration
                {
                    MediaConvertorOptions = new MediaConvertorOptions(),
                    PluginProcessOptions = new PluginProcessOptions(),
                    Scripting = new Scripting(),
                    InboundDirectory = @"/home/steven/incoming/melodee_test/tests",
                    StagingDirectory = string.Empty,
                    LibraryDirectory = string.Empty
                }
            );
            var convertorResult = await convertor.ProcessFileAsync(fileInfo.ToFileSystemInfo());
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
        }
    }  
}