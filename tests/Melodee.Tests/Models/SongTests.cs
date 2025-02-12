using Melodee.Common.Enums;
using Melodee.Common.Models;
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

    [Fact]
    public void ValidateValuesFromMetaTags()
    {
        var album = AlbumValidatorTests.TestAlbum;
        Assert.NotNull(album.Songs?.FirstOrDefault());

        var firstSong = album.Songs!.First();
        
        var albumTitle1 = firstSong.AlbumTitle();
        var albumTitle2 = album.MetaTagValue<string>(MetaTagIdentifier.Album);
        Assert.Equal("Cold Spring Harbor", albumTitle1);
        Assert.Equal(albumTitle1, albumTitle2);
        
        var title = firstSong.MetaTagValue<string>(MetaTagIdentifier.Title);
        Assert.Equal("She's Got a Way", title);

        var year = firstSong.MetaTagValue<int>(MetaTagIdentifier.RecordingYear);
        Assert.Equal(1971, year);
        
        var yearDateTime = firstSong.MetaTagValue<DateTime>(MetaTagIdentifier.RecordingDate);
        Assert.Equal(DateTime.Parse("11/01/1971"), yearDateTime);        
        
        var numberofMediasInt = firstSong.MetaTagValue<int>(MetaTagIdentifier.DiscTotal);
        Assert.Equal(1, numberofMediasInt);        
        
        var numberofMedias = firstSong.MetaTagValue<short>(MetaTagIdentifier.DiscTotal);
        Assert.Equal(1, numberofMedias);
        
        album.SetSongTagValue(firstSong.Id, MetaTagIdentifier.SongTotal, 10);
        firstSong = album.Songs!.First(x => x.Id == firstSong.Id);
        var numberofSongs = firstSong.MetaTagValue<short>(MetaTagIdentifier.SongTotal);
        Assert.Equal(10, numberofSongs);

        short trackNumberValue = 34;
        album.SetSongTagValue(firstSong.Id, MetaTagIdentifier.TrackNumber, trackNumberValue);
        firstSong = album.Songs!.First(x => x.Id == firstSong.Id);
        var trackNumber = firstSong.MetaTagValue<int>(MetaTagIdentifier.TrackNumber);
        Assert.Equal(34, trackNumber);
        
        
    }

    [Fact]
    public void ValidateMergeBest()
    {
        var song = AlbumValidatorTests.TestAlbum.Songs!.First();

        var betterMediaAudios = song.MediaAudios?.ToList() ?? [];
        betterMediaAudios.Remove(betterMediaAudios.First(x => x.Identifier == MediaAudioIdentifier.BitRate));
        betterMediaAudios.Add(new MediaAudio<object?> { Identifier = MediaAudioIdentifier.BitRate, Value = 999 });

        var betterTags = song.Tags?.ToList() ?? [];
        betterTags.Add(new MetaTag<object?> { Identifier = MetaTagIdentifier.Publisher, Value = "Bobs Publishing" });

        var betterSong = song with
        {
            MediaAudios = betterMediaAudios,
            Tags = betterTags
        };
        var mergedSong = Song.IdentityBestAndMergeOthers([song, betterSong]);
        Assert.Equal(song.AlbumTitle(), mergedSong.AlbumTitle());
        Assert.Equal(999, mergedSong.MediaAudios?.First(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value);
        Assert.NotNull(mergedSong.Tags);
        Assert.Contains(mergedSong.Tags, x => x.Identifier == MetaTagIdentifier.Publisher && x.Value as string == "Bobs Publishing");
    }
}
