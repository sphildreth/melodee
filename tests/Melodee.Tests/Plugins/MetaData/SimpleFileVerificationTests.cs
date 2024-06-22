using Melodee.Plugins.MetaData.Release.Models;
using SimpleFileVerification = Melodee.Plugins.MetaData.Release.SimpleFileVerification;

namespace Melodee.Tests.Plugins.MetaData;

public class SimpleFileVerificationTests
{
    [Fact]
    public async Task ValidateSfvFileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test.sfv";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var sfv = new SimpleFileVerification();
            var sfvResult = await sfv.ProcessFileAsync(fileInfo);
            Assert.NotNull(sfvResult);
            Assert.True(sfvResult.IsSuccess);
            Assert.NotNull(sfvResult.Data);

            var release = sfvResult.Data;
            Assert.NotNull(release);
            Assert.NotNull(release.Tracks);
            Assert.NotEmpty(release.Tracks);
        }
    }

    [Fact]
    public void ValidateModelFullLineParsing()
    {
        // <trackNumber>-<releaseArtist>-<trackTitle>.mp3 <crcHash>
        var input = "01-avatar-bound_to_the_wall.mp3 7a84ce20";
        var shouldBe = new SfvLine
        {
            IsValid = false,
            CrcHash = "7a84ce20",
            FileInfo = new FileInfo("01-avatar-bound_to_the_wall.mp3"),
            ReleaseArist = "Avatar",
            TrackTitle = "Bound To The Wall",
            TrackNumber = 1
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(@"M:\_bad_or_missing_folder\file.sfv", input);
        Assert.Equal(shouldBe, parsedModel);
    }

    [Fact]
    public void ValidateModelNoReleaseTitleLineParsing()
    {
        var input = "01-pole_shift.mp3 aff033ca";
        var shouldBe = new SfvLine
        {
            IsValid = false,
            CrcHash = "aff033ca",
            FileInfo = new FileInfo("01-pole_shift.mp3"),
            TrackTitle = "Pole Shift",
            TrackNumber = 1
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(@"file.sfv",input);
        Assert.Equal(shouldBe, parsedModel);
    }    
    
    [Fact]
    public void ValidateModelOnlyFilenameLineParsing()
    {
        var input = "pole_shift.mp3 aff033ca";
        var shouldBe = new SfvLine
        {
            IsValid = false,
            CrcHash = "aff033ca",
            FileInfo = new FileInfo("pole_shift.mp3"),
            TrackTitle = "Pole Shift",
            TrackNumber = 0
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(string.Empty,input);
        Assert.Equal(shouldBe, parsedModel);
    }       
}