using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Track;
using SimpleFileVerification = Melodee.Plugins.MetaData.Directory.SimpleFileVerification;

namespace Melodee.Tests.Plugins.MetaData;

public class M3UTests
{
    [Fact]
    public async Task ValidateM3UFileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.m3u";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var m3U = new M3UPlaylist(new []
            {
                new MetaTag(TestsBase.NewConfiguration)
            }, TestsBase.NewConfiguration);
            var m3UResult = await m3U.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/inbound/00-k 2024",
                Name = "00-k 2024"
            });
            Assert.NotNull(m3UResult);
            Assert.NotNull(m3UResult);
            Assert.True(m3UResult.IsSuccess);
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
            FileSystemFileInfo = new FileSystemFileInfo
            {
              Path  = string.Empty,
              Name = "01-avatar-bound_to_the_wall.mp3",
              Size = 12345
            },
            ReleaseArist = "Avatar",
            TrackTitle = "Bound To The Wall",
            TrackNumber = 1
        };
        var parsedModel = M3UPlaylist.ModelFromM3ULine(@"M:\_bad_or_missing_folder\file.sfv", input);
        Assert.Equal(shouldBe, parsedModel);
    }    

    
}