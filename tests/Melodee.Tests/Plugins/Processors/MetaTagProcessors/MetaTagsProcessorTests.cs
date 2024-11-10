using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Plugins.Processor;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class MetaTagsProcessorTests : TestsBase
{
    [Theory]
    [InlineData("Something", 0, "Something")]
    [InlineData("Something 1", 0, "Something 1")] // Single digits are ignored as they are likely part of the Song name
    [InlineData("14 Something", 14, "Something")]
    [InlineData("008 Something", 8, "Something")]
    [InlineData("08 Something", 8, "Something")]
    public async Task ValidateSongTitleSongNumberRemoved(string? originalSongTitle, int SongNumber, string? shouldBe)
    {
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.TrackNumber, Value = SongNumber },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Title, Value = originalSongTitle }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var SongTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title);
        Assert.NotNull(SongTag);
        Assert.NotNull(SongTag.Value);
        Assert.Equal(shouldBe, SongTag.Value);
        if (SongNumber > 0)
        {
            var SongNumberTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber);
            Assert.NotNull(SongNumberTag);
            Assert.NotNull(SongNumberTag.Value);
            Assert.Equal(SongNumber, SongNumberTag.Value);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Something", "Something")]
    [InlineData("Eternally Gifted", "Eternally Gifted")]
    [InlineData("Shift Scene", "Shift Scene")]
    [InlineData("Something with Bob", "Something")]
    [InlineData("Something Ft Bob", "Something")]
    [InlineData("Something ft Bob", "Something")]
    [InlineData("Something Ft. Bob", "Something")]
    [InlineData("Something (Ft. Bob)", "Something")]
    [InlineData("Something Feat. Bob", "Something")]
    [InlineData("Something Featuring Bob", "Something")]
    [InlineData("Something (with Bob)", "Something")]
    [InlineData("Minds Without Fear with Vishal-Shekhar", "Minds Without Fear")]
    [InlineData("Actin Bad Baby (feat. Brookie Salas)", "Actin Bad Baby")]
    [InlineData("Actin Bad Baby (with Stinky Feet and Smelly Feet)", "Actin Bad Baby")]
    [InlineData("Actin Bad Baby (ft. Bob; Stinky Feet;Smelly Feet)", "Actin Bad Baby")]
    [InlineData("Rain (Stealing Sheep Remix)", "Rain (Stealing Sheep Remix)")]
    [InlineData("In My Arms (Crumbling Down Extended Instrumental Remix;Remaster)", "In My Arms (Crumbling Down Extended Instrumental Remix)")]
    [InlineData("404", "404")]
    [InlineData("With", "With")]
    public async Task ValidateSongTitleFeaturingRemoved(string? originalSongTitle, string? shouldBe)
    {
        var albumArtistShouldBe = "Da Artist";
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = albumArtistShouldBe },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.TrackNumber, Value = 27 },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Title, Value = originalSongTitle }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var SongTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title);
        Assert.NotNull(SongTag);

        if (shouldBe != null)
        {
            Assert.NotNull(SongTag.Value);
            if (SongTag.Value as string != originalSongTitle)
            {
                Assert.NotNull(SongTag.OriginalValue);
                Assert.Equal(originalSongTitle, SongTag.OriginalValue);
            }

            Assert.Equal(shouldBe, SongTag.Value);
            var albumArtist = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist);
            if (albumArtist != null)
            {
                Assert.NotNull(albumArtist.Value);
                Assert.Equal(albumArtistShouldBe, albumArtist.Value);
            }
        }
        else
        {
            Assert.Null(SongTag.Value);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Eternal Sunshine (Slightly Deluxe)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Deluxe)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (DELUXE)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine [DELUXE]", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Enhanced Japanese Edition)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Complete Edition)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Expanded Edition)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Disk 2)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Disc 2)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (CD 2)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine - CD 2", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine [Disk 2])", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (Japan Edition)", "Eternal Sunshine")]
    [InlineData("Eternal Sunshine (40th anniversary edition)", "Eternal Sunshine")]
    [InlineData("Exodus (1977 original Album)", "Exodus")]
    [InlineData("Eternally Gifted", "Eternally Gifted")]
    [InlineData("Shift Scene", "Shift Scene")]
    [InlineData("Shift Scene ^##^", "Shift Scene")]
    [InlineData("[1984] Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("4 [2011]", "4")]
    [InlineData("1984", "1984")]
    [InlineData("13", "13")]
    [InlineData("Big Hits (High Tide and Green Grass)", "Big Hits (High Tide and Green Grass)")]
    [InlineData("Billion Dollar Babies (50th Anniversary Deluxe Edition)", "Billion Dollar Babies")]
    [InlineData("Billion Dollar Babies (Deluxe Edition)", "Billion Dollar Babies")]
    [InlineData("Axis: Bold As Love (2010 Remaster)", "Axis: Bold As Love")]
    [InlineData("Homework (25th Anniversary Edition) (1)", "Homework")]
    [InlineData("Balearic Classics Vol. 1", "Balearic Classics Vol. 1")]
    [InlineData("Live At Leeds (Deluxe Edition)", "Live At Leeds")]
    [InlineData("The Dark Side Of The Moon (50th Anniversary) [2023 Remaster]", "The Dark Side Of The Moon")]
    [InlineData("Me (Radio Edit)", "Me (Radio Edit)")]
    public async Task ValidateAlbumTitleUnwantedRemoved(string? originalAlbum, string? shouldBe)
    {
        var albumArtistShouldBe = "Da Artist";
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = albumArtistShouldBe },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = originalAlbum }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var SongTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album);
        if (shouldBe != null)
        {
            Assert.NotNull(SongTag);
            Assert.NotNull(SongTag.Value);


            if (SongTag.Value as string != originalAlbum)
            {
                Assert.NotNull(SongTag.OriginalValue);
                Assert.Equal(originalAlbum, SongTag.OriginalValue);
            }

            Assert.Equal(shouldBe, SongTag.Value);
        }
        else
        {
            Assert.Null(SongTag);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Ariana Grande", null)]
    [InlineData("Ariana Grande ft Nonna", "Nonna")]
    [InlineData("Ariana Grande ft Mariah Carey", "Mariah Carey")]
    [InlineData("AC DC", "AC/DC")]
    [InlineData("Love/Hate", "Love/Hate")]
    public async Task ValidateAlbumArtistValue(string? originalArtist, string? shouldBe)
    {
        var albumArtistShouldBe = "Ariana Grande";
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = albumArtistShouldBe },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = originalArtist }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var songTitleArtistTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist);
        if (originalArtist.DoStringsMatch(albumArtistShouldBe))
        {
            Assert.Null(songTitleArtistTag);
        }
        else
        {
            Assert.NotNull(songTitleArtistTag);

            if (shouldBe != null)
            {
                Assert.NotNull(songTitleArtistTag.Value);
                if (songTitleArtistTag.Value as string != originalArtist)
                {
                    Assert.NotNull(songTitleArtistTag.OriginalValue);
                    Assert.Equal(originalArtist, songTitleArtistTag.OriginalValue);
                }
            }
            else
            {
                Assert.Null(songTitleArtistTag.Value);
            }

            Assert.Equal(shouldBe, songTitleArtistTag.Value);
        }
    }


    [Theory]
    [InlineData("Ariana Grande", "Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Ariana Grande Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Ariana Grande - Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Ariana Grande : Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Ariana Grande.Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Ariana Grande . Eternal Sunshine", "Eternal Sunshine")]
    public async Task ValidateAlbumTitleDoesntContainAlbumArtist(string? albumArtist, string? albumTitle, string? shouldBe)
    {
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = albumArtist },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = albumTitle }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var albumTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album);
        Assert.NotNull(albumTag);
        Assert.Equal(shouldBe, albumTag.Value);
    }

    [Theory]
    [InlineData("Ariana Grande", null, "Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Nonna", "Nonna Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", null, "Ariana Grande - Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", "Nonna", "Ariana Grande Nonna : Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", null, "Ariana Grande.Eternal Sunshine", "Eternal Sunshine")]
    [InlineData("Ariana Grande", null, "Ariana Grande . Eternal Sunshine", "Eternal Sunshine")]
    public async Task ValidateAlbumTitleDoesntContainArtist(string? albumArtist, string? SongArtist, string? albumTitle, string? shouldBe)
    {
        var processor = new MetaTagsProcessor(NewPluginsConfiguration(), Serializer);
        var processorResult = await processor.ProcessMetaTagAsync(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = albumArtist },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = SongArtist },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = albumTitle }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var albumTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album);
        Assert.NotNull(albumTag);
        Assert.Equal(shouldBe, albumTag.Value);
    }
}
