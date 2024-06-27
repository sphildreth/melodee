using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Models;
using SimpleFileVerification = Melodee.Plugins.MetaData.Directory.SimpleFileVerification;

namespace Melodee.Tests.Plugins.MetaData;

public class M3UTests
{
    [Fact]
    public async Task ValidateM3UFileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test.m3u";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var sfv = new M3UPlaylist(TestsBase.NewConfiguration);
            var m3UResult = await sfv.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            });
            Assert.NotNull(m3UResult);
            // As there are no tracks on the release there should be SFV missing messages and the result is of type validation failure.
            Assert.False(m3UResult.IsSuccess);
            Assert.NotNull(m3UResult.Data);
        }
    }
    
    [Fact]
    public void ValidateModelFullLineParsing()
    {
        // <trackNumber>-<releaseArtist>-<trackTitle>.mp3
        var input = "01-avatar-bound_to_the_wall.mp3";
        var shouldBe = new M3ULine()
        {
            IsValid = false,
            FileInfo = new FileInfo("01-avatar-bound_to_the_wall.mp3"),
            ReleaseArist = "Avatar",
            TrackTitle = "Bound To The Wall",
            TrackNumber = 1
        };
        var parsedModel = M3UPlaylist.ModelFromM3ULine(@"M:\_bad_or_missing_folder\file.sfv", input);
        Assert.Equal(shouldBe, parsedModel);
    }    

    
}