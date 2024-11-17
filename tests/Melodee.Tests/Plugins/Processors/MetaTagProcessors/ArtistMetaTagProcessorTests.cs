using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.Processor.MetaTagProcessors;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class ArtistMetaTagProcessorTests : TestsBase
{
    [Fact]
    public void ValidateArtistNameWithFeaturing()
    {
        var AlbumArtistShouldBe = "Ariana Grande";
        var SongArtistShouldBe = "Nonna";

        var metatagProcessor = new Artist(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Artist,
            Value = "Ariana Grande ft Nonna"
        };
        var result = metatagProcessor.ProcessMetaTag(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, tag, []);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data, x => x.Identifier == MetaTagIdentifier.AlbumArtist);
        Assert.Contains(result.Data, x => x.Identifier == MetaTagIdentifier.Artist);
        Assert.Equal(AlbumArtistShouldBe, result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist)?.Value);
        Assert.Equal(SongArtistShouldBe, result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist)?.Value);
    }

    [Fact]
    public void ValidateArtistNameWithFeaturingWithAlbumArtistSet()
    {
        var metatagProcessor = new Artist(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Artist,
            Value = "Ariana Grande ft Nonna"
        };
        var result = metatagProcessor.ProcessMetaTag(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        }, new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, tag, [
            new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.AlbumArtist,
                Value = "Ariana Grande"
            }
        ]);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal("Nonna", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist)?.Value);
    }
}
