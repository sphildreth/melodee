using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Track;
using CueSheet = Melodee.Plugins.MetaData.Directory.CueSheet;

namespace Melodee.Tests.Plugins.MetaData;

public class CUETests
{
    [Fact]
    public async Task ValidateSfvFileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/Pixel_-_Reality_Strikes_Back-2004-MYCEL/01-pixel_-_reality_strikes_back-mycel.cue";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var sfv = new CueSheet(
                new []
                {
                    new MetaTag(TestsBase.NewConfiguration)
                }, TestsBase.NewConfiguration);
            var sfvResult = await sfv.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/inbound/Pixel_-_Reality_Strikes_Back-2004-MYCEL",
                Name = "Pixel_-_Reality_Strikes_Back-2004-MYCEL"
            });
            Assert.NotNull(sfvResult);
            Assert.True(sfvResult.IsSuccess);
        }
    }
   
}