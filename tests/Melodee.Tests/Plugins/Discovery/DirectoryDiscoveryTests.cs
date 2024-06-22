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
        var dd = new DirectoriesDiscoverer();
        var directoryInfosForDirectory = dd.DirectoryInfosForDirectory(new DirectoryInfo(Guid.NewGuid().ToString()), new PagedRequest());
        Assert.NotNull(directoryInfosForDirectory);
        Assert.False(directoryInfosForDirectory.IsSuccess);
        Assert.NotNull(directoryInfosForDirectory);
        Assert.NotNull(directoryInfosForDirectory.Data);
        Assert.Empty(directoryInfosForDirectory.Data);
    }
    
    [Fact]
    public void ValidateIfTestDirectorySetupNotEmpty()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/inbound";
        //var testDirectory = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var dir = new System.IO.DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var dd = new DirectoriesDiscoverer();
            var directoryInfosForDirectory = dd.DirectoryInfosForDirectory(dir, new PagedRequest());
            Assert.NotNull(directoryInfosForDirectory);
            Assert.True(directoryInfosForDirectory.IsSuccess);
            Assert.NotNull(directoryInfosForDirectory);
            Assert.NotNull(directoryInfosForDirectory.Data);
            Assert.NotEmpty(directoryInfosForDirectory.Data);
            Assert.DoesNotContain(directoryInfosForDirectory.Data, x => x.MusicFilesFound == 0);
        }
    }
}