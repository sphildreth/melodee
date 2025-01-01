using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Media;
using Melodee.Common.Plugins.Conversion.Media;

namespace Melodee.Tests.Plugins.Conversion;

public class MediaConvertorTests
{
    [Fact]
    public async Task ValidateConvertingFlacToMp3Async()
    {
        var testFile = @"/melodee_test/tests/testflac.flac";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new MediaConvertor(TestsBase.NewPluginsConfiguration());
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/tests/",
                Name = "tests"
            };
            var convertorResult = await convertor.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);

            var convertedFileInfo = new FileInfo(convertorResult.Data.FullName(dirInfo));
            Assert.True(convertedFileInfo.Exists);
        }
    }

    [Fact]
    public async Task ValidateConvertingNonMediaFailsAsync()
    {
        var testFile = @"/melodee_test/tests/testbmp.bmp";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new MediaConvertor(new MelodeeConfiguration(
                new Dictionary<string, object?>())
            );
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/tests/",
                Name = "tests"
            };
            var convertorResult = await convertor.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
        }
    }
}
