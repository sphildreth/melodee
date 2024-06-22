using Melodee.Common.Models;
using Melodee.Plugins.Discovery.Directories;
using Melodee.Plugins.Discovery.Releases;

namespace Melodee.Tests.Plugins.Discovery;

public class ReleasesDiscovererTests
{
    [Fact]
    public async Task ValidReleaseGridResults()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/inbound";
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

            var rd = new ReleasesDiscoverer();
            var releasesForDirectoryAsync = await rd.ReleasesGridsForDirectoryAsync(directoryInfosForDirectory.Data.First(x => x.ParentId > 9 && x.MusicFilesFound > 0), new PagedRequest());
            Assert.NotNull(releasesForDirectoryAsync);
            Assert.True(releasesForDirectoryAsync.IsSuccess);

            var releases = releasesForDirectoryAsync.Data;
            Assert.NotNull(releases);        
            Assert.NotEmpty(releases);

            var firstRelease = releases.First();

            Assert.True(firstRelease.TrackCount > 1);
            Assert.True(firstRelease.Year > 0);

        }

    }
}