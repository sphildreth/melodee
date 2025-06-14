using ATL;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
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
            var atlFromFlac = new Track(fileInfo.FullName);
            var configuration = TestsBase.NewPluginsConfiguration();
            configuration.SetSetting(SettingRegistry.ConversionEnabled, true);
            configuration.SetSetting(SettingRegistry.ProcessingConvertedExtension, string.Empty);
            var convertor = new MediaConvertor(configuration);
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

            var atlFromMp3 = new Track(convertedFileInfo.FullName);

            Assert.Equal(atlFromFlac.Duration, atlFromMp3.Duration);
            Assert.Equal(atlFromFlac.Artist, atlFromMp3.Artist);
            Assert.Equal(atlFromFlac.Album, atlFromMp3.Album);
            Assert.Equal(atlFromFlac.TrackNumber, atlFromMp3.TrackNumber);

            File.Delete(convertedFileInfo.FullName);
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
