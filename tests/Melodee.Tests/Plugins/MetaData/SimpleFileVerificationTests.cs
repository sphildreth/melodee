using System.Runtime.CompilerServices;
using Melodee.Plugins.MetaData.Release;

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
            var sfv = new Melodee.Plugins.MetaData.Release.SimpleFileVerification();
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
        var input = "01-avatar-bound_to_the_wall.mp3 7a84ce20";
        var shouldBe = new SimpleFileVerification.SfvLine
        {
            IsValid = false,
            CrcHash = "7a84ce20",
            FileInfo = new FileInfo("01-avatar-bound_to_the_wall.mp3"),
            ReleaseTitle = "Avatar",
            TrackTitle = "Bound To The Wall",
            TrackNumber = 1
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(input);
        Assert.Equal(shouldBe, parsedModel);
    }

    [Fact]
    public void ValidateModelNoReleaseTitleLineParsing()
    {
        var input = "01-pole_shift.mp3 aff033ca";
        var shouldBe = new SimpleFileVerification.SfvLine
        {
            IsValid = false,
            CrcHash = "aff033ca",
            FileInfo = new FileInfo("01-pole_shift.mp3"),
            TrackTitle = "Pole Shift",
            TrackNumber = 1
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(input);
        Assert.Equal(shouldBe, parsedModel);
    }    
    
    [Fact]
    public void ValidateModelOnlyFilenameLineParsing()
    {
        var input = "pole_shift.mp3 aff033ca";
        var shouldBe = new SimpleFileVerification.SfvLine
        {
            IsValid = false,
            CrcHash = "aff033ca",
            FileInfo = new FileInfo("pole_shift.mp3"),
            TrackTitle = "Pole Shift",
            TrackNumber = 0
        };
        var parsedModel = SimpleFileVerification.ModelFromSfvLine(input);
        Assert.Equal(shouldBe, parsedModel);
    }       
}