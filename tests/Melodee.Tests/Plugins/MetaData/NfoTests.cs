using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Track;
using CueSheet = Melodee.Plugins.MetaData.Directory.CueSheet;

namespace Melodee.Tests.Plugins.MetaData;

public class NfoTests
{
    [Fact]
    public async Task ParseNfoFile()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.nfo";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(
                new []
                {
                    new MetaTag( TestsBase.NewConfiguration)
                }, TestsBase.NewConfiguration);
            var nfoParserResult = await nfo.ReleaseForNfoFileAsync(testFile);
            Assert.NotNull(nfoParserResult);
            Assert.True(nfoParserResult.IsValid());
        }
    }
   
}