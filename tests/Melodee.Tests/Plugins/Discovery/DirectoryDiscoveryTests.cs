using Melodee.Common.Models;
using Melodee.Plugins.Discovery.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Melodee.Tests.Plugins.Discovery;

public class DirectoryDiscoveryTests
{
    [Theory]
    [InlineData("mp3", true)]
    [InlineData("MP3", true)]
    [InlineData("Mp3", true)]
    [InlineData(".mp3", true)]
    [InlineData("m3u", false)]
    [InlineData("sfv", false)]
    [InlineData("mxx", false)]
    [InlineData("txt", false)]
    [InlineData("jpg", false)]
    [InlineData("png", false)]
    public void ValidateIsMediaTypeFile(string extension, bool shouldBe) =>
        Assert.Equal(shouldBe, DirectoryDiscoverer.IsFileMediaType(extension));

    [Fact]
    public void ValidateIsEmptyResult()
    {
        var dd = new DirectoryDiscoverer();
        var filesForDirectory =
            dd.DirectoryInfosForDirectory(new DirectoryInfo(Guid.NewGuid().ToString()), new PagedRequest());
        Assert.NotNull(filesForDirectory);
        Assert.False(filesForDirectory.IsSuccess);
        Assert.NotNull(filesForDirectory.Data);
        Assert.NotNull(filesForDirectory.Data.Rows);
        Assert.Empty(filesForDirectory.Data.Rows);
    }
    
    [Fact]
    public void ValidateIfTestDirectorySetupNotEmpty()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/inbound";
        var dir = new System.IO.DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var dd = new DirectoryDiscoverer();
            var filesForDirectory =
                dd.DirectoryInfosForDirectory(dir, new PagedRequest());
            Assert.NotNull(filesForDirectory);
            Assert.True(filesForDirectory.IsSuccess);
            Assert.NotNull(filesForDirectory.Data);
            Assert.NotNull(filesForDirectory.Data.Rows);
            Assert.NotEmpty(filesForDirectory.Data.Rows);
            Assert.DoesNotContain(filesForDirectory.Data.Rows, x => x.MusicFilesFound == 0);
        }
    }
}