using Melodee.Common.Enums;
using Melodee.Common.Exceptions;
using Melodee.Common.Models;
using SmartFormat.Utilities;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class ArtistMetaTagProcessorTests
{
    [Fact]
    public void ValidateArtistNameWithFeaturing()
    {
        var releaseArtistShouldBe = "Ariana Grande";
        var trackArtistShouldBe = "Nonna";
        
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.Artist(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Artist,
            Value = "Ariana Grande ft Nonna"
        };
        var result = metatagProcessor.ProcessMetaTag(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        },new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, tag, []);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Data, x => x.Identifier == MetaTagIdentifier.AlbumArtist);
        Assert.Contains(result.Data, x => x.Identifier == MetaTagIdentifier.Artist);
        Assert.Equal(releaseArtistShouldBe, result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist)?.Value);
        Assert.Equal(trackArtistShouldBe, result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist)?.Value);
    }
    
    [Fact]
    public void ValidateArtistNameWithFeaturingWithAlbumArtistSet()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.Artist(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Artist,
            Value = "Ariana Grande ft Nonna"
        };
        var result = metatagProcessor.ProcessMetaTag(new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = string.Empty
        },new FileSystemFileInfo
        {
            Name = string.Empty,
            Size = 0
        }, tag, [new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.AlbumArtist,
            Value = "Ariana Grande"
        }]);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal("Nonna", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist)?.Value);
    }    
}
