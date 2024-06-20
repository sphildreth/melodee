using Melodee.Common.Models;
using Melodee.Plugins.Discovery.Directories;
using Melodee.Plugins.Discovery.Releases;

namespace Melodee.Tests.Plugins.Discovery;

public class ReleasesDiscovererTests
{
    [Fact]
    public async void ValidReleaseGridResults()
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
            Assert.NotNull(filesForDirectory.Data);
            Assert.NotEmpty(filesForDirectory.Data);
            Assert.DoesNotContain(filesForDirectory.Data, x => x.MusicFilesFound == 0);

            var rd = new ReleasesDiscoverer();
            var releasesForDirectoryAsync = await rd.ReleasesForDirectoryAsync(filesForDirectory.Data.First(), new PagedRequest());
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