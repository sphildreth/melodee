using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services.Scanning;

namespace Melodee.Tests.Services;

public class AlbumDiscoveryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ValidAlbumGridInboundResults()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/inbound";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var rd = new AlbumDiscoveryService(
                Logger,
                CacheManager,
                MockFactory(),
                GetSettingService(),
                Serializer);
            await rd.InitializeAsync();
            var albumsForDirectoryAsync = await rd.AlbumsGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/inbound",
                Name = "staging"
            }, new PagedRequest());
            Assert.NotNull(albumsForDirectoryAsync);
            Assert.True(albumsForDirectoryAsync.IsSuccess);
            Assert.DoesNotContain(albumsForDirectoryAsync.Data, x => x.UniqueId == 0);

            var albums = albumsForDirectoryAsync.Data.ToArray();
            Assert.NotNull(albums);        
            Assert.NotEmpty(albums);
    
            var firstAlbum = albums.First();
    
            Assert.True(firstAlbum.SongCount > 1);
            Assert.True(firstAlbum.Year > 0);
            Assert.NotNull(firstAlbum.Duration?.Nullify());
    
        }
    
    }    
    
    [Fact]
    public async Task ValidAlbumGridStagingResults()
    {
        var testDirectory = @"/home/steven/incoming/melodee_test/staging";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var rd = new AlbumDiscoveryService(
                Logger,
                CacheManager,
                MockFactory(),
                GetSettingService(),
                Serializer);
            await rd.InitializeAsync();
            var albumsForDirectoryAsync = await rd.AlbumsGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/staging",
                Name = "staging"
            }, new PagedRequest());
            Assert.NotNull(albumsForDirectoryAsync);
            Assert.True(albumsForDirectoryAsync.IsSuccess);
            Assert.DoesNotContain(albumsForDirectoryAsync.Data, x => x.UniqueId == 0);

            var albums = albumsForDirectoryAsync.Data.ToArray();
            Assert.NotNull(albums);        
            Assert.NotEmpty(albums);
    
            var firstAlbum = albums.First();
    
            Assert.True(firstAlbum.SongCount > 1);
            Assert.True(firstAlbum.Year > 0);
            Assert.NotNull(firstAlbum.Duration?.Nullify());
    
        }
    
    }
}
