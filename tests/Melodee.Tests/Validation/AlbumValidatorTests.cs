using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Validation;

namespace Melodee.Tests.Validation;

public class AlbumValidatorTests
{
    private static Album TestAlbum
        => new()
        {
            Directory = new FileSystemDirectoryInfo
            {
                Path = "/melodee_test/tests/",
                Name = "tests"
            },
            ViaPlugins = [nameof(AtlMetaTag)],
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = string.Empty,
                Name = string.Empty
            },
            Status = AlbumStatus.Ok,
            Images = new []
            {
                new ImageInfo
                {
                    CrcHash = "12345",
                    FileInfo = new FileSystemFileInfo
                    {
                        Name = "2020591499-01-Front.jpg",
                        Size = 12345,
                    },
                    PictureIdentifier = PictureIdentifier.Front
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
                }
            },
            Songs = new[]
            {
                new Song
                {
                    CrcHash = "TestValue",
                    File = new FileSystemFileInfo
                    {
                        Name = string.Empty,
                        Size = 12345
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
                            Value = "1"
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = "She's Got a Way"
                        }
                    }
                }
            }
        };

    [Theory]
    [InlineData("Album Title", "Something", 1, false)]
    [InlineData("Album Title", "11:11", 5, false)]
    [InlineData("Album Title", "11:11", 11, false)]
    [InlineData("Album Title", "The Song Title", 5, false)]
    [InlineData(null, null, 1, true)]
    [InlineData(null, "", 1, true)]
    [InlineData(null, " ", 1, true)]
    [InlineData(null, "   ", 1, true)]
    [InlineData("Album Title", "15", 15, true)]
    [InlineData("Album Title", "15 ", 15, true)]
    [InlineData("Album Title", "0005 Song Title", 5, true)]
    [InlineData("Album Title", "Song   Title", 5, true)]
    [InlineData("Album Title", "11 - Song Title", 11, true)]
    [InlineData("Album Title", "Song Title - Part II", 11, false)]
    [InlineData("Album Title", "Album Title", 1, false)]
    [InlineData("Album Title", "Album Title - 01 Song Title", 1, true)]
    [InlineData("Album Title", "I Can't Even Walk Without You Holding My Hand", 6, false)]
    [InlineData("Album Title", "'81 Camaro", 1, false)]
    [InlineData("Album Title", "'81 Camaro", 8, false)]
    [InlineData("Album Title", "'81 Camaro", 81, false)]
    [InlineData("Album Title", "Album Title (prod DJ Stinky)", 5, true)]
    [InlineData("Album Title", "Production Blues", 5, false)]
    [InlineData("Album Title", "The Production Blues", 5, false)]
    [InlineData("Album Title", "Deep Delightful (DJ Andy De Gage Remix)", 5, false)]
    [InlineData("Album Title", "Left and Right (Feat. Jung Kook of BTS)", 5, true)]
    [InlineData("Album Title", "Left and Right ft. Jung Kook)", 5, true)]
    [InlineData("Album Title", "Karakondžula", 5, false)]
    [InlineData("Album Title", "Song■Title", 5, false)]
    [InlineData("Album Title", "Song💣Title", 5, false)]
    [InlineData("Album Title best of 48 years (Compiled and Mixed by DJ Stinky", "Song Title (Compiled and Mixed by DJ Stinky)", 5, false)]
    [InlineData("Megamix Chart Hits Best Of 12 Years (Compiled and Mixed by DJ Fl", "Megamix Chart Hits Best Of 12 Years (Compiled and Mixed by DJ Flimflam)", 5, false)]
    [InlineData("Album Title", "Flowers (Demo)", 11, false)]
    public void SongHasUnwantedText(string? AlbumTitle, string? SongName, int? SongNumber, bool shouldBe)
    {
        Assert.Equal(shouldBe, AlbumValidator.SongHasUnwantedText(AlbumTitle, SongName, SongNumber));
    }

    [Theory]
    [InlineData("A Stone's Throw", false)]
    [InlineData("Broken Arrow", false)]
    [InlineData("American Music Vol. 1", false)]
    [InlineData("Retro", false)]
    [InlineData("Eternally Gifted", false)]
    [InlineData("Electric Deluge, Vol. 2", false)]
    [InlineData("Album■Title", false)]
    [InlineData("Album💣Title", false)]
    [InlineData("The Fine Art Of Self Destruction", false)]
    [InlineData("Experience Yourself", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("   ", true)]
    [InlineData("Album Title Digipak", true)]
    [InlineData("Album Title digipak", true)]
    [InlineData("Album Title diGIpaK", true)]
    [InlineData("Monarch Deluxe Edition", true)]
    [InlineData("Monarch Re-Master", true)]
    [InlineData("Monarch Target Edition", true)]
    [InlineData("Monarch Remastered", true)]
    [InlineData("Monarch Re-mastered", true)]
    [InlineData("Monarch Album", true)]
    [InlineData("Monarch Remaster", true)]
    [InlineData("Monarch Reissue", true)]
    [InlineData("Monarch (REISSUE)", true)]
    [InlineData("Monarch Expanded", true)]
    [InlineData("Monarch (Expanded)", true)]
    [InlineData("Monarch (Expanded", true)]
    [InlineData("Monarch WEB", true)]
    [InlineData("Monarch REMASTERED", true)]
    [InlineData("Monarch (REMASTERED)", true)]
    [InlineData("Monarch [REMASTERED]", true)]
    [InlineData("Michael Bublé - Higher (Deluxe)", true)]
    [InlineData("Necro Sapiens (320)", true)]
    [InlineData("Necro Sapiens Compilation", true)]
    [InlineData("Necro Sapiens (Compilation)", true)]
    [InlineData("Captain Morgan's Revenge(Limited Japanese Editiom) (10th Anniversary Edition)", true)]
    [InlineData("Captain Morgan's Revenge (Limited Japanese Editiom) (10th Anniversary Edition)", true)]
    [InlineData("Hard work (EP)", true)]
    [InlineData("Hard work EP", true)]
    [InlineData("Experience Yourself Ep", true)]
    [InlineData("Experience Yourself LP", true)]
    [InlineData("Experience Yourself (Single)", true)]
    [InlineData("Escape (Deluxe Edition)", true)]
    [InlineData("Escape (Deluxe)", true)]
    [InlineData("Arsenal of Glory (Re-Edition)", true)]
    [InlineData("Arsenal of Glory (2005 Edition)", true)]
    public void AlbumTitleHasUnwantedText(string? AlbumTitle, bool shouldBe)
    {
        Assert.Equal(shouldBe, AlbumValidator.AlbumTitleHasUnwantedText(AlbumTitle));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("Something", false)]
    [InlineData("Eternally Gifted", false)]
    [InlineData("Shift Scene", false)]
    [InlineData("Something With Bob", true)]
    [InlineData("Something Ft Bob", true)]
    [InlineData("Something ft Bob", true)]
    [InlineData("Something Ft. Bob", true)]
    [InlineData("Something (Ft. Bob)", true)]
    [InlineData("Something Feat. Bob", true)]
    [InlineData("Something Featuring Bob", true)]
    [InlineData("Something (with Bob)", true)]
    [InlineData("Minds Without Fear with Vishal-Shekhar", true)]
    public void StringHasFeaturingFragments(string? input, bool shouldBe)
    {
        Assert.Equal(AlbumValidator.StringHasFeaturingFragments(input), shouldBe);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("Song Title", false)]
    [InlineData("cover-PROOF.jpg", true)]
    [InlineData("cover proof.jpg", true)]
    [InlineData("proof.jpg", true)]
    [InlineData("proof image.jpg", true)]
    [InlineData("00-master_blaster-we_love_italo_disco-cd-flac-2003-proof.jpg", true)]
    [InlineData("cover.jpg", false)]
    [InlineData("00-big_ed_the_assassin-edward_lee_knight_1971-2001-2001-proof-cr_int", true)]
    public void IsImageProofType(string? text, bool shouldBe)
    {
        Assert.Equal(shouldBe, AlbumValidator.IsImageAProofType(text));
    }


    [Fact]
    public void ValidateAlbumWithNoInvalidValidations()
    {
        var Album = TestAlbum;
        var validator = new AlbumValidator(TestsBase.NewPluginsConfiguration());
        var validationResult = validator.ValidateAlbum(Album);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(Album.Status, validationResult.Data.AlbumStatus);
    }

    [Fact]
    public void ValidateAlbumWithNoCoverImage()
    {
        var testAlbum = TestAlbum;
        var album = new Album
        {
            Directory = testAlbum.Directory,
            Tags = testAlbum.Tags,
            Songs = testAlbum.Songs,
            ViaPlugins = testAlbum.ViaPlugins,
            OriginalDirectory = testAlbum.OriginalDirectory
        };

        var validator = new AlbumValidator(TestsBase.NewPluginsConfiguration());
        var validationResult = validator.ValidateAlbum(album);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(AlbumStatus.Invalid, validationResult.Data.AlbumStatus);
    }
    
    [Fact]
    public void ValidateAlbumWithMissingArtist()
    {
        var testAlbum = TestAlbum;
        var albumTags = (testAlbum.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        albumTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.AlbumArtist,
            Value = "Billy Joel"
        });
        var album = new Album
        {
            Directory = testAlbum.Directory,
            Tags = albumTags,
            Songs = testAlbum.Songs,
            ViaPlugins = testAlbum.ViaPlugins,
            OriginalDirectory = testAlbum.OriginalDirectory
        };

        var validator = new AlbumValidator(TestsBase.NewPluginsConfiguration());
        var validationResult = validator.ValidateAlbum(album);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(AlbumStatus.Invalid, validationResult.Data.AlbumStatus);
    }

    [Fact]
    public void ValidateAlbumWithInvalidYear()
    {
        var testAlbum = TestAlbum;
        var albumTags = (testAlbum.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        albumTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.RecordingYear,
            Value = "1971"
        });
        var album = new Album
        {
            Directory = testAlbum.Directory,
            Tags = albumTags,
            Songs = testAlbum.Songs,
            ViaPlugins = testAlbum.ViaPlugins,
            OriginalDirectory = testAlbum.OriginalDirectory
        };

        var validator = new AlbumValidator(TestsBase.NewPluginsConfiguration());
        var validationResult = validator.ValidateAlbum(album);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(AlbumStatus.Invalid, validationResult.Data.AlbumStatus);
    }

    [Fact]
    public void ValidateAlbumWithMissingTitle()
    {
        var testAlbum = TestAlbum;
        var albumTags = (testAlbum.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        albumTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Album,
            Value = "Cold Spring Harbor"
        });
        var album = new Album
        {
            Directory = testAlbum.Directory,
            Tags = albumTags,
            Songs = testAlbum.Songs,
            ViaPlugins = testAlbum.ViaPlugins,
            OriginalDirectory = testAlbum.OriginalDirectory
        };

        var validator = new AlbumValidator(TestsBase.NewPluginsConfiguration());
        var validationResult = validator.ValidateAlbum(album);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(AlbumStatus.Invalid, validationResult.Data.AlbumStatus);
    }

    [Theory]
    [InlineData("A simple song title", "A simple song title")]
    [InlineData("Flowers   (DEMO)", "Flowers (DEMO)")]
    [InlineData("Bless em With The Blade (Orchestral Version)", "Bless em With The Blade (Orchestral Version)")]
    public void ValidateSongTitleReplacement(string input, string shouldBe)
    {
        Assert.Equal(shouldBe, AlbumValidator.RemoveUnwantedTextFromSongTitle(input));
    }
}
