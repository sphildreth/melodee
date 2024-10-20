using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.Validation;

namespace Melodee.Tests.Plugins.Discovery;

public class ReleasesDiscovererTests
{
    [Fact]
    public async Task ValidReleaseGridResults()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/staging";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var rd = new ReleasesDiscoverer(new ReleaseValidator(TestsBase.NewConfiguration), TestsBase.NewConfiguration);
            var releasesForDirectoryAsync = await rd.ReleasesGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/staging",
                Name = "staging"
            }, new PagedRequest());
            Assert.NotNull(releasesForDirectoryAsync);
            Assert.True(releasesForDirectoryAsync.IsSuccess);
            Assert.DoesNotContain(releasesForDirectoryAsync.Data, x => x.UniqueId == 0);

            var releases = releasesForDirectoryAsync.Data.ToArray();
            Assert.NotNull(releases);        
            Assert.NotEmpty(releases);
    
            var firstRelease = releases.First();
    
            Assert.True(firstRelease.TrackCount > 1);
            Assert.True(firstRelease.Year > 0);
            Assert.NotNull(firstRelease.Duration?.Nullify());
    
        }
    
    }
}
