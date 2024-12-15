using System.Diagnostics;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Nfo;

namespace Melodee.Tests.Plugins.MetaData;

public class NfoTests : TestsBase
{
    [Fact]
    public async Task ParseNfoFile()
    {
        var testFile = @"/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(Serializer, TestsBase.NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.True(nfoParserResult.IsValid(TestsBase.NewConfiguration()).Item1);
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("bob", false)]
    [InlineData("лллллллллллллллллммллллллллллллВл  ллллплллВА   олллл     АВллллллА    ппВВВллл", false)]
    [InlineData("01 The Cloverland Panopticon 03:28", true)]
    [InlineData("1 The Cloverland Panopticon 3:28", true)]
    [InlineData("\ud83d\udea8 01 The Cloverland Panopticon 3:28", true)]
    [InlineData("01 > The Cloverland Panopticon                                 < 03:28", true)]
    [InlineData("           ллл   01.Tokyo Night                          05:25   ллл", true)]
    [InlineData("Û 02. Legendary (feat. Lonnie Westry)                                     2:41 Û", true)]
    [InlineData("             01. Szango ....................................... 05:52  ", true)]
    public void ValidateIsLineForSong(string? input, bool shouldBe)
    {
        Assert.Equal(shouldBe, Nfo.IsLineForSong(input ?? string.Empty));
    }
    
    [Fact]
    public async Task ParseNfoFile01()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        
        var testFile = @"/melodee_test/tests/test_nfo01.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(Serializer, TestsBase.NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.NotNull(nfoParserResult.Songs);
            Assert.True(nfoParserResult.MediaCountValue() > 0);
        }
    }
}
