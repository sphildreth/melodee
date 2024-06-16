using Melodee.Common.Models;
using Melodee.Plugins.Discovery;
using Melodee.Plugins.Discovery.Directories;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Melodee.Tests.Plugins.Discovery;

public class DirectoryDiscoveryTests
{

    [Fact]
    public void ValidateIsEmptyResult()
    {
        var dd = new DirectoriesesDiscoverer();
        var filesForDirectory =
            dd.DirectoryInfosForDirectory(new DirectoryInfo(Guid.NewGuid().ToString()), new PagedRequest());
        Assert.NotNull(filesForDirectory);
        Assert.False(filesForDirectory.IsSuccess);
        Assert.NotNull(filesForDirectory);
        Assert.NotNull(filesForDirectory.Rows);
        Assert.Empty(filesForDirectory.Rows);
    }
    
    [Fact]
    public void ValidateIfTestDirectorySetupNotEmpty()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/inbound";
        var dir = new System.IO.DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var dd = new DirectoriesesDiscoverer();
            var filesForDirectory =
                dd.DirectoryInfosForDirectory(dir, new PagedRequest());
            Assert.NotNull(filesForDirectory);
            Assert.True(filesForDirectory.IsSuccess);
            Assert.NotNull(filesForDirectory);
            Assert.NotNull(filesForDirectory.Rows);
            Assert.NotEmpty(filesForDirectory.Rows);
            Assert.DoesNotContain(filesForDirectory.Rows, x => x.MusicFilesFound == 0);
        }
    }
}