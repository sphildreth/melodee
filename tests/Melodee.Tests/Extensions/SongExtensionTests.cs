using Melodee.Common.Enums;
using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class SongExtensionTests
{
    [Fact]
    public void ValidateSongNewFileName()
    {
        var album = AlbumExtensionTests.NewAlbum();
        var songNewFileName = album.Songs!.First().ToSongFileName(album.Directory);
        Assert.NotNull(songNewFileName);
        Assert.Equal(@"0003 Flako El Dark Cowboy.mp3", songNewFileName);
    }

    [Fact]
    public void ValidateSongFileName()
    {
        var album = AlbumExtensionTests.NewAlbum();
        var songNewFileName = album.Songs!.First().ToSongFileName(album.Directory);
        Assert.NotNull(songNewFileName);
        Assert.Equal(@"0003 Flako El Dark Cowboy.mp3", songNewFileName);
        
        album.SetSongTagValue(album.Songs!.First().Id, MetaTagIdentifier.Title, "Bob Loves Nancy!");
        Assert.Equal(@"0003 Bob Loves Nancy!.mp3", album.Songs!.First().ToSongFileName(album.Directory));
        
        album.SetSongTagValue(album.Songs!.First().Id, MetaTagIdentifier.Title, "$$$");
        Assert.Equal(@"0003 __x24f__x24f__x24f.mp3", album.Songs!.First().ToSongFileName(album.Directory));
    }
}
