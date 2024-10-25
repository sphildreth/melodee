using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Plugins.Discovery.Albums;
using Melodee.Plugins.Validation;

namespace Melodee.Tests.Plugins.Discovery;

public class AlbumsDiscovererTests : TestsBase
{
    [Fact]
    public async Task ValidAlbumGridResults()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/staging";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var rd = new AlbumsDiscoverer(new AlbumValidator(TestsBase.NewConfiguration), TestsBase.NewConfiguration, Serializer);
            var AlbumsForDirectoryAsync = await rd.AlbumsGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/staging",
                Name = "staging"
            }, new PagedRequest());
            Assert.NotNull(AlbumsForDirectoryAsync);
            Assert.True(AlbumsForDirectoryAsync.IsSuccess);
            Assert.DoesNotContain(AlbumsForDirectoryAsync.Data, x => x.UniqueId == 0);

            var Albums = AlbumsForDirectoryAsync.Data.ToArray();
            Assert.NotNull(Albums);        
            Assert.NotEmpty(Albums);
    
            var firstAlbum = Albums.First();
    
            Assert.True(firstAlbum.SongCount > 1);
            Assert.True(firstAlbum.Year > 0);
            Assert.NotNull(firstAlbum.Duration?.Nullify());
    
        }
    
    }
}
