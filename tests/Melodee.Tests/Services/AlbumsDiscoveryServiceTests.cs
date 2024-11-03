using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services.Scanning;

namespace Melodee.Tests.Services;

public class AlbumDiscoveryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ValidAlbumGridInboundResults()
    {
        var testDirectory = @"/melodee_test/inbound";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var rd = new AlbumDiscoveryService(
                Logger,
                CacheManager,
                MockFactory(),
                GetSettingService(),
                Serializer);
            await rd.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var albumsForDirectoryAsync = await rd.AlbumsGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/inbound",
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
        var testDirectory = @"/melodee_test/staging";
        var dir = new DirectoryInfo(testDirectory);
        // Only run if the test staging directory exists and if the test staging directory has directories (otherwise no albums to return).
        if (dir.Exists && Directory.GetDirectories(testDirectory).Length > 0)
        {
            var rd = new AlbumDiscoveryService(
                Logger,
                CacheManager,
                MockFactory(),
                GetSettingService(),
                Serializer);
            await rd.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var albumsForDirectoryAsync = await rd.AlbumsGridsForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/staging",
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
