using FluentValidation;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Validation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Melodee.Tests.Validation;

public class ReleaseValidatorTests
{
    [Theory]
    [InlineData("Release Title", "Something", 1, false)]
    [InlineData("Release Title", "11:11", 5, false)]
    [InlineData("Release Title", "11:11", 11, false)]
    [InlineData("Release Title", "The Track Title", 5, false)]
    [InlineData(null, null, 1, true)]
    [InlineData(null, "", 1, true)]
    [InlineData(null, " ", 1, true)]
    [InlineData(null, "   ", 1, true)]
    [InlineData("Release Title", "15", 15, true)]
    [InlineData("Release Title", "15 ", 15, true)]
    [InlineData("Release Title", "0005 Track Title", 5, true)]
    [InlineData("Release Title", "Track   Title", 5, true)]
    [InlineData("Release Title", "11 - Track Title", 11, true)]
    [InlineData("Release Title", "Track Title - Part II", 11, false)]
    [InlineData("Release Title", "Release Title", 1, false)]
    [InlineData("Release Title", "Release Title - 01 Track Title", 1, true)]
    [InlineData("Release Title", "I Can't Even Walk Without You Holding My Hand", 6, false)]
    [InlineData("Release Title", "'81 Camaro", 1, false)]
    [InlineData("Release Title", "'81 Camaro", 8, false)]
    [InlineData("Release Title", "'81 Camaro", 81, false)]
    [InlineData("Release Title", "Release Title (prod DJ Stinky)", 5, true)]
    [InlineData("Release Title", "Production Blues", 5, false)]
    [InlineData("Release Title", "The Production Blues", 5, false)]
    [InlineData("Release Title", "Deep Delightful (DJ Andy De Gage Remix)", 5, false)]
    [InlineData("Release Title", "Left and Right (Feat. Jung Kook of BTS)", 5, true)]
    [InlineData("Release Title", "Left and Right ft. Jung Kook)", 5, true)]
    [InlineData("Release Title", "Karakondžula", 5, false)]
    [InlineData("Release Title", "Track■Title", 5, false)]
    [InlineData("Release Title", "Track💣Title", 5, false)]
    [InlineData("Release Title best of 48 years (Compiled and Mixed by DJ Stinky", "Track Title (Compiled and Mixed by DJ Stinky)", 5, false)]
    [InlineData("Megamix Chart Hits Best Of 12 Years (Compiled and Mixed by DJ Fl", "Megamix Chart Hits Best Of 12 Years (Compiled and Mixed by DJ Flimflam)", 5, false)]
    public void TrackHasUnwantedText(string? releaseTitle, string? trackName, int? trackNumber, bool shouldBe)
    {
        Assert.Equal(shouldBe, ReleaseValidator.TrackHasUnwantedText(releaseTitle, trackName, trackNumber));
    }

    [Theory]
    [InlineData("A Stone's Throw", false)]
    [InlineData("Broken Arrow", false)]
    [InlineData("American Music Vol. 1", false)]
    [InlineData("Retro", false)]
    [InlineData("Eternally Gifted", false)]
    [InlineData("Electric Deluge, Vol. 2", false)]
    [InlineData("Release■Title", false)]
    [InlineData("Release💣Title", false)]
    [InlineData("The Fine Art Of Self Destruction", false)]
    [InlineData("Experience Yourself", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("   ", true)]
    [InlineData("Release Title Digipak", true)]
    [InlineData("Release Title digipak", true)]
    [InlineData("Release Title diGIpaK", true)]
    [InlineData("Monarch Deluxe Edition", true)]
    [InlineData("Monarch Re-Master", true)]
    [InlineData("Monarch Target Edition", true)]
    [InlineData("Monarch Remastered", true)]
    [InlineData("Monarch Re-mastered", true)]
    [InlineData("Monarch Release", true)]
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
    public void ReleaseTitleHasUnwantedText(string? releaseTitle, bool shouldBe)
    {
        Assert.Equal(shouldBe, ReleaseValidator.ReleaseTitleHasUnwantedText(releaseTitle));
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
        Assert.Equal(ReleaseValidator.StringHasFeaturingFragments(input), shouldBe);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("Track Title", false)]
    [InlineData("cover-PROOF.jpg", true)]
    [InlineData("cover proof.jpg", true)]
    [InlineData("proof.jpg", true)]
    [InlineData("proof image.jpg", true)]
    [InlineData("00-master_blaster-we_love_italo_disco-cd-flac-2003-proof.jpg", true)]
    [InlineData("cover.jpg", false)]
    [InlineData("00-big_ed_the_assassin-edward_lee_knight_1971-2001-2001-proof-cr_int", true)]
    public void IsImageProofType(string? text, bool shouldBe)
    {
        Assert.Equal(shouldBe, ReleaseValidator.IsImageAProofType(text));
    }

    private static Release TestRelease
        => new Release
        {
            ViaPlugins = [nameof(AtlMetaTag)],
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = string.Empty,
                Name = string.Empty
            },
            Status = ReleaseStatus.Ok,
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
            Tracks = new[]
            {
                new Track
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
                        },
                    },
                }
            }
        };

   
    [Fact]
    public void ValidateReleaseWithNoInvalidValidations()
    {
        var release = TestRelease;
        var validator = new ReleaseValidator(TestsBase.NewConfiguration);
        var validationResult = validator.ValidateRelease(release);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(release.Status, validationResult.Data.ReleaseStatus);
    }

    [Fact]
    public void ValidateReleaseWithMissingArtist()
    {
        var testRelease = TestRelease;
        var releaseTags = (testRelease.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        releaseTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.AlbumArtist,
            Value = "Billy Joel"
        });
        var release = new Release
        {
            Tags = releaseTags,
            Tracks = testRelease.Tracks,
            ViaPlugins = testRelease.ViaPlugins,
            OriginalDirectory = testRelease.OriginalDirectory
        };

        var validator = new ReleaseValidator(TestsBase.NewConfiguration);
        var validationResult = validator.ValidateRelease(release);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(ReleaseStatus.Invalid, validationResult.Data.ReleaseStatus);
    }

    [Fact]
    public void ValidateReleaseWithInvalidYear()
    {
        var testRelease = TestRelease;
        var releaseTags = (testRelease.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        releaseTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.RecordingYear,
            Value = "1971"
        });
        var release = new Release
        {
            Tags = releaseTags,
            Tracks = testRelease.Tracks,
            ViaPlugins = testRelease.ViaPlugins,
            OriginalDirectory = testRelease.OriginalDirectory
        };

        var validator = new ReleaseValidator(TestsBase.NewConfiguration);
        var validationResult = validator.ValidateRelease(release);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(ReleaseStatus.Invalid, validationResult.Data.ReleaseStatus);
    }

    [Fact]
    public void ValidateReleaseWithMissingTitle()
    {
        var testRelease = TestRelease;
        var releaseTags = (testRelease.Tags ?? Array.Empty<MetaTag<object?>>()).ToList();
        releaseTags.Remove(new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Album,
            Value = "Cold Spring Harbor"
        });
        var release = new Release
        {
            Tags = releaseTags,
            Tracks = testRelease.Tracks,
            ViaPlugins = testRelease.ViaPlugins,
            OriginalDirectory = testRelease.OriginalDirectory
        };

        var validator = new ReleaseValidator(TestsBase.NewConfiguration);
        var validationResult = validator.ValidateRelease(release);
        Assert.True(validationResult.IsSuccess);
        Assert.Equal(ReleaseStatus.Invalid, validationResult.Data.ReleaseStatus);
    }
}
