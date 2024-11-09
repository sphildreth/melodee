using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class SongExtensionTests
{
    [Fact]
    public void ValidateSongNewFileName()
    {
        var album = AlbumExtensionTests.NewAlbum();
        var songNewFileName = album.Songs!.First().ToSongFileName(TestsBase.NewConfiguration());
        Assert.NotNull(songNewFileName);
        Assert.Equal(@"003 Flako El Dark Cowboy.mp3", songNewFileName);
    }
}
