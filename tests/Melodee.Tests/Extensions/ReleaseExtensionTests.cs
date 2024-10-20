using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Tests.Extensions;

public class ReleaseExtensionTests
{
    public static Release NewRelease()
    {
        return new Release
        {
            ViaPlugins = [],
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/inbound/00-k 2024",
                Name = "00-k 2024"
            },
            Tags = new[]
            {
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.UniqueArtistId,
                    Value = 12345L
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = "Holy Truth"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.OrigReleaseYear,
                    Value = 2024
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.DiscNumberTotal,
                    Value = "1/2"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.Album,
                    Value = "Fire Proof"
                }
            },
            Tracks = new[]
            {
                new Track
                {
                    CrcHash = CRC32.Calculate(new FileInfo(@"/home/steven/incoming/melodee_test/inbound/00-k 2024/03-holy_truth-flako_el_dark_cowboy.mp3")),
                    File = new FileSystemFileInfo
                    {
                        Name = "03-holy_truth-flako_el_dark_cowboy.mp3\"",
                        Size = 12343
                    },
                    Tags = new[]
                    {
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = "Holy Truth"
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.TrackNumber,
                            Value = 3
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.TrackTotal,
                            Value = 1
                        },
                        new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = "Flako El Dark Cowboy"
                        }
                    }
                }
            }
        };
    }

    [Theory]
    [InlineData(@"/home/steven/incoming/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.sfv", true)]
    [InlineData(@"/home/steven/incoming/melodee_test/inbound/00-k 2024/03-holy_truth-flako_el_dark_cowboy.mp3", true)]
    [InlineData(@"/home/steven/incoming/melodee_test/inbound/00-k 2024/00--fire_proof-(dzb707)-web-2024.sfv", false)]
    [InlineData(@"/home/steven/incoming/melodee_test/inbound/00-k 2024/00-kittie-vultures-ep-web-2024.sfv", false)]
    [InlineData("batman", false)]
    public void ValidateFileIsForRelease(string fileName, bool shouldBe)
    {
        if (File.Exists(fileName))
        {
            Assert.Equal(shouldBe, NewRelease().IsFileForRelease(new FileInfo(fileName)));
        }
    }

    [Fact]
    public void ValidateArtistDirectoryName()
    {
        var artistDirectoryName = NewRelease().ArtistDirectoryName(TestsBase.NewConfiguration);
        Assert.NotNull(artistDirectoryName);
        Assert.Equal(12345L, NewRelease().ArtistUniqueId());
        Assert.Equal(@"H/HO/Holy Truth [12345]", artistDirectoryName);
    }

    [Fact]
    public void ValidateReleaseDirectoryName()
    {
        var artistDirectoryName = NewRelease().ReleaseDirectoryName(TestsBase.NewConfiguration);
        Assert.NotNull(artistDirectoryName);
        Assert.Equal(12345L, NewRelease().ArtistUniqueId());
        Assert.Equal(@"H/HO/Holy Truth [12345]/[2024] Fire Proof", artistDirectoryName);
    }

    [Fact]
    public void ValidateDiscTotalValue()
    {
        Assert.Equal(2, NewRelease().MediaCountValue());
    }
    
    [Fact]
    public void ValidateTrackTotalValueUsingTrackTotal()
    {
        Assert.Equal(1, NewRelease().TrackTotalValue());
    } 
   
}
