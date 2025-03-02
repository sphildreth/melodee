using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Directory.Nfo;

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
            var nfo = new Nfo(Serializer, GetAlbumValidator(), NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.Equal(3, nfoParserResult.Songs?.Count());
            var dir = fileInfo.Directory?.ToDirectorySystemInfo();
            Assert.NotNull(dir);
            Assert.NotNull(nfoParserResult.Songs);
            Assert.DoesNotContain(nfoParserResult.Songs, x => !x.File.Exists(dir));
        }
    }

    [Fact]
    public async Task ParseNfoFile2()
    {
        var testFile = @"/melodee_test/inbound/00-k 2024/00-kittie-vultures-ep-web-2024.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(Serializer, GetAlbumValidator(), NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.Equal(3, nfoParserResult.Songs?.Count());
            var dir = fileInfo.Directory?.ToDirectorySystemInfo();
            Assert.NotNull(dir);
            Assert.NotNull(nfoParserResult.Songs);
            Assert.DoesNotContain(nfoParserResult.Songs, x => !x.File.Exists(dir));
        }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("bob", false)]
    [InlineData("лллллллллллллллллммллллллллллллВл  ллллплллВА   олллл     АВллллллА    ппВВВллл", false)]
    [InlineData("                   Holy_Truth-Fire_Proof-(DZB707)-WEB-2024-PTC", false)]
    [InlineData("           \ufffd\ufffd\ufffd   Quality...: 320kbps ~ 44,1kHz ~ Joint Stereo    \ufffd\ufffd\ufffd", false)]
    [InlineData("\ufffd\ufffd\ufffd   Length....: 20:49                               \ufffd\ufffd\ufffd", false)]
    [InlineData("01 The Cloverland Panopticon 03:28", true)]
    [InlineData("1 The Cloverland Panopticon 3:28", true)]
    [InlineData("\ud83d\udea8 01 The Cloverland Panopticon 3:28", true)]
    [InlineData("           \ufffd\ufffd\ufffd   01.Pole Shift                           07:24   \ufffd\ufffd\ufffd", true)]
    [InlineData("01 > The Cloverland Panopticon                                 < 03:28", true)]
    [InlineData("           ллл   01.Tokyo Night                          05:25   ллл", true)]
    [InlineData("Û 02. Legendary (feat. Lonnie Westry)                                     2:41 Û", true)]
    [InlineData("             01. Szango ....................................... 05:52  ", true)]
    public void ValidateIsLineForSong(string? input, bool shouldBe)
    {
        Assert.Equal(shouldBe, Nfo.IsLineForSong(input ?? string.Empty));
    }

    [Fact]
    public async Task ParseJellyfinNfoFile()
    {
        var testFile = @"/melodee_test/tests/jellyfin_album.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(Serializer, GetAlbumValidator(), NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.NotNull(nfoParserResult);
            Assert.Equal("Chris Young", nfoParserResult.Artist.Name);
            Assert.True(nfoParserResult.Status == AlbumStatus.New);
        }
    }

    [Fact]
    public async Task ParseNfoFile01()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        var testFile = @"/melodee_test/tests/test_nfo01.nfo";
        var fileInfo = new FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var nfo = new Nfo(Serializer, GetAlbumValidator(), NewPluginsConfiguration());
            var nfoParserResult = await nfo.AlbumForNfoFileAsync(fileInfo, fileInfo.Directory?.ToDirectorySystemInfo());
            Assert.Null(nfoParserResult);
            Assert.Null(nfoParserResult?.Songs);
        }
    }
}
