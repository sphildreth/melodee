using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class SongExtensionTests
{
    [Fact]
    public void ValidateSongNewFileName()
    {
        var Album = AlbumExtensionTests.NewAlbum();
        var SongNewFileName = Album.Songs!.First().ToSongFileName(TestsBase.NewConfiguration());
        Assert.NotNull(SongNewFileName);
        Assert.Equal(@"003 Flako El Dark Cowboy.mp3", SongNewFileName);
    }
}
