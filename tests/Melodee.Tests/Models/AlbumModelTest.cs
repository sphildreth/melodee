using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Tests.Validation;
using ServiceStack;

namespace Melodee.Tests.Models;

public class AlbumModelTest
{
    [Fact]
    public void ValidateMergeDuplicateIsNotDuplicated()
    {
        var album1 = AlbumValidatorTests.TestAlbum;
        var album1SongCount = album1.Songs.Count();
        
        var album2 = AlbumValidatorTests.TestAlbum;
        
        var merged = album1.Merge(album2);
        Assert.Equal(album1SongCount, merged.Songs.Count());
        
        var songIds = merged.Songs.Select(x => x.SongUniqueId()).ToList();
        Assert.Equal(album1SongCount, songIds.Count());
    }
    
    [Fact]
    public void ValidateAlbumsMerge()
    {
        var album1 = AlbumValidatorTests.TestAlbum;

        var album2Songs = album1.Songs.ToList();
        album2Songs.Add(new Song
        {
            CrcHash = "TestValue3",
            File = new FileSystemFileInfo
            {
                Name = string.Empty,
                Size = 234567
            },
            MediaAudios = new[]
            {
                new MediaAudio<object?>
                {
                    Identifier = MediaAudioIdentifier.BitRate,
                    Value = AlbumValidatorTests.ShouldBeBitRate
                }
            },
            Tags = new[]
            {
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = "Billy Joel"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Album,
                    Value = "Cold Spring Harbor"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.RecordingYear,
                    Value = "1971"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.TrackNumber,
                    Value = "3"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Title,
                    Value = "Everybody Loves You Now"
                }
            }
        });

        var album2 = album1 with
        {
            Songs = album2Songs
        };
        
        var merged = album1.Merge(album2);
        Assert.Equal(3, merged.Songs.Count());
        
        Assert.Contains(merged.Songs, x => x.CrcHash == "TestValue3");
        Assert.Equal(3, merged.SongTotalValue());
    }
}
