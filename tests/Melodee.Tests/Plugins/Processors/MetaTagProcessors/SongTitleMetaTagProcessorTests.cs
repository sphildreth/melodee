using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.Processor.MetaTagProcessors;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class SongTitleMetaTagProcessorTests : TestsBase
{
    [Fact]
    public void ValidateSongTitleWithFeaturing()
    {
        var metatagProcessor = new SongTitle(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Some name (ft Nonna)"
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
        Assert.Equal("Some Name", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }

    [Fact]
    public void ValidateSongTitleWithWithShouldRemove1()
    {
        var metatagProcessor = new SongTitle(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Fly Way (with Bob Jones)"
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
        Assert.Equal("Fly Way", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }

    [Fact]
    public void ValidateSongTitleWithWithShouldRemove2()
    {
        var metatagProcessor = new SongTitle(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Fly Way with Bob Jones"
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
        Assert.Equal("Fly Way", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }

    [Fact]
    public void ValidateSongTitleWithWithShouldStay()
    {
        var metatagProcessor = new SongTitle(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "With Me (Radio Edit)"
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
        Assert.Equal("With Me (Radio Edit)", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }
}
