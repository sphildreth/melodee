using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Utility;

namespace Melodee.Tests.Extensions;

public class AlbumExtensionTests : TestsBase
{
    public static Album NewAlbum()
    {
        return new Album
        {
            Artist = new Artist(
                "Holy Truth",
                "Holy Truth".ToNormalizedString()!,
                null),
            Directory = new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/inbound/00-k 2024",
                Name = "00-k 2024"
            },
            ViaPlugins = [],
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = @"/melodee_test/inbound/00-k 2024",
                Name = "00-k 2024"
            },
            Tags = new[]
            {
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = "Holy Truth"
                },
                new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.RecordingYear,
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
            Songs = new[]
            {
                new Song
                {
                    CrcHash = Crc32.Calculate(new FileInfo(@"/melodee_test/inbound/00-k 2024/03-holy_truth-flako_el_dark_cowboy.mp3")),
                    File = new FileSystemFileInfo
                    {
                        Name = "03-holy_truth-flako_el_dark_cowboy.mp3",
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
                            Identifier = MetaTagIdentifier.SongTotal,
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
    [InlineData(@"/melodee_test/inbound/00-k 2024/00-holy_truth-fire_proof-(dzb707)-web-2024.sfv", true)]
    [InlineData(@"/melodee_test/inbound/bingo bango/i-01-Front.jpg", true)]
    [InlineData(@"/melodee_test/inbound/00-k 2024/03-holy_truth-flako_el_dark_cowboy.mp3", true)]
    [InlineData(@"/melodee_test/inbound/00-k 2024/00--fire_proof-(dzb707)-web-2024.sfv", false)]
    [InlineData(@"/melodee_test/inbound/00-k 2024/00-kittie-vultures-ep-web-2024.sfv", false)]
    [InlineData("batman", false)]
    public async Task ValidateFileIsForAlbum(string fileName, bool shouldBe)
    {
        if (File.Exists(fileName))
        {
            var config = await MockConfigurationFactory().GetConfigurationAsync();
            Assert.Equal(shouldBe, NewAlbum()
                    .IsFileForAlbum(new AtlMetaTag(new MetaTagsProcessor(config, Serializer), GetImageConvertor(), GetImageValidator(), config),
                        new FileInfo(fileName)));
        }
    }

    [Fact]
    public void ValidateSongTotalValueUsingSongTotal()
    {
        Assert.Equal(1, NewAlbum().SongTotalValue());
    }
}
