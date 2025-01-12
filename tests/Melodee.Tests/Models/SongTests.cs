using Melodee.Common.Enums;
using Melodee.Common.Models.Extensions;
using Melodee.Tests.Validation;

namespace Melodee.Tests.Models;

public class SongTests
{
    [Fact]
    public void ValidateEmptyMusicBrainzReturnsNull()
    {
        var album = AlbumValidatorTests.TestAlbum;
        Assert.NotNull(album.Songs?.FirstOrDefault());

        var firstSongId = album.Songs!.First().Id;

        var noValueShouldBeNull = album.Songs!.First(x => x.Id == firstSongId).MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId);
        Assert.Null(noValueShouldBeNull);

        album.SetSongTagValue(firstSongId, MetaTagIdentifier.MusicBrainzId, Guid.Empty);
        var emptyGuidShouldBeNull = album.Songs!.First(x => x.Id == firstSongId).MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId);
        Assert.Null(emptyGuidShouldBeNull);

        var shouldBe = Guid.NewGuid();
        album.SetSongTagValue(firstSongId, MetaTagIdentifier.MusicBrainzId, shouldBe);
        var shouldMatchShouldBe = album.Songs!.First(x => x.Id == firstSongId).MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId);
        Assert.Equal(shouldBe, shouldMatchShouldBe);
    }
}
