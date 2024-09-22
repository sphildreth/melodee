using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using SimpleFileVerification = Melodee.Plugins.MetaData.Directory.SimpleFileVerification;

namespace Melodee.Tests.Plugins.MetaData;

public class M3UTests
{
    [Fact]
    public async Task ValidateM3UFileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.m3u";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var m3U = new M3UPlaylist(new []
            {
                new AtlMetaTag(new MetaTagsProcessor(TestsBase.NewConfiguration), TestsBase.NewConfiguration)
            }, new ReleaseValidator(TestsBase.NewConfiguration),
               TestsBase.NewConfiguration);
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
    public void ValidateModelFullLineParsing3()
    {
        // <trackNumber>-<releaseArtist>-<trackTitle>.mp3
        var input = "01-avatar-bound_to_the_wall.mp3";
        var shouldBe = new M3ULine()
        {
            IsValid = false,
            FileSystemFileInfo = new FileSystemFileInfo
            {
              Name = "01-avatar-bound_to_the_wall.mp3",
              Size = 0
            },
            ReleaseArist = "Avatar",
            TrackTitle = "Bound To The Wall",
            TrackNumber = 1
        };
        var parsedModel = M3UPlaylist.ModelFromM3ULine(string.Empty, input);
        Assert.Equal(shouldBe, parsedModel);
    }    

    [Fact]
    public void ValidateModelFullLineParsing4()
    {
        // <trackNumber>-<releaseArtist>-<trackTitle>-<crc>.mp3
        var input = "01-kittie-vultures-9f80b183.mp3";
        var shouldBe = new M3ULine()
        {
            IsValid = false,
            FileSystemFileInfo = new FileSystemFileInfo
            {
                Name = "01-kittie-vultures-9f80b183.mp3",
                Size = 0
            },
            ReleaseArist = "Kittie",
            TrackTitle = "Vultures",
            TrackNumber = 1
        };
        var parsedModel = M3UPlaylist.ModelFromM3ULine(string.Empty, input);
        Assert.Equal(shouldBe, parsedModel);
    }      

    
}
