using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Directory;

namespace Melodee.Tests.Plugins.MetaData;

public class NfoTests
{
    [Fact]
    public async Task ParseNfoFile()
    {
        var testFile = @"/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(TestsBase.NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.True(nfoParserResult.IsValid(TestsBase.NewConfiguration()));
        }
    }

    [Fact]
    public async Task ParseNfoFile01()
    {
        var testFile = @"/melodee_test/tests/test_nfo01.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(TestsBase.NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.NotNull(nfoParserResult.Songs);
            Assert.True(nfoParserResult.IsValid(TestsBase.NewConfiguration()));

            Assert.DoesNotContain(nfoParserResult.Songs, x => x.Title() != null && x.Title()!.Contains("..."));
        }
    }
}
