using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services.Scanning;

namespace Melodee.Tests.Services;

public class AlbumDiscoveryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ValidAlbumGridInboundResults()
    {
        var testDirectory = @"/melodee_test/staging";
        var dir = new DirectoryInfo(testDirectory);
        if (dir.Exists)
        {
            var hasMelodeeFiles = dir.GetFiles("*.melodee.json");
            if (hasMelodeeFiles.Any())
            {
                var rd = new AlbumDiscoveryService(
                    Logger,
                    CacheManager,
                    MockFactory(),
                    MockConfigurationFactory(),
                    Serializer);
                await rd.InitializeAsync(TestsBase.NewPluginsConfiguration());
                var albumsForDirectoryAsync = await rd.AlbumsDataInfosForDirectoryAsync(new FileSystemDirectoryInfo
                {
                    Path = @"/melodee_test/staging",
                    Name = "staging"
                }, new PagedRequest());
                Assert.NotNull(albumsForDirectoryAsync);
                Assert.True(albumsForDirectoryAsync.IsSuccess);

                var albums = albumsForDirectoryAsync.Data.ToArray();
                Assert.NotNull(albums);
                Assert.NotEmpty(albums);

                var firstAlbum = albums.First();

                Assert.True(firstAlbum.SongCount > 1);
                Assert.True(firstAlbum.ReleaseDate.Year > 0);
                Assert.True(firstAlbum.Duration > 0);
            }
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
                MockConfigurationFactory(),
                Serializer);
            await rd.InitializeAsync(TestsBase.NewPluginsConfiguration());
            var albumsForDirectoryAsync = await rd.AlbumsDataInfosForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/staging",
                Name = "staging"
            }, new PagedRequest());
            Assert.NotNull(albumsForDirectoryAsync);
            Assert.True(albumsForDirectoryAsync.IsSuccess);

            var albums = albumsForDirectoryAsync.Data.ToArray();
            Assert.NotNull(albums);
            Assert.NotEmpty(albums);

            var firstAlbum = albums.First();

            Assert.True(firstAlbum.SongCount > 1);
            Assert.True(firstAlbum.ReleaseDate.Year > 0);
            Assert.True(firstAlbum.Duration > 0);
        }
    }
}
