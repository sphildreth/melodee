using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;

using Melodee.Common.Models.Extensions;
using Melodee.Plugins;

namespace Melodee.Tests.Plugins.Conversion;

public class MediaConvertorTests
{
    [Fact] public async Task ValidateConvertingFlacToMp3Async()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/testflac.flac";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Conversion.Media.MediaConvertor(TestsBase.NewConfiguration());
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
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
        var testFile = @"/home/steven/incoming/melodee_test/tests/testbmp.bmp";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
             var convertor = new Melodee.Plugins.Conversion.Media.MediaConvertor(
                 new Dictionary<string, object?>
                {
                    { SettingRegistry.DirectoryInbound, @"/home/steven/incoming/melodee_test/tests" },
                    { SettingRegistry.DirectoryStaging, string.Empty },
                    { SettingRegistry.DirectoryLibrary, string.Empty }
                }
            );
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            };            
            var convertorResult = await convertor.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
        }
    }  
}
