using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.Processor.MetaTagProcessors;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class AlbumTitleMetaTagProcessorTests : TestsBase
{
    [Fact]
    public void ValidateAlbumTitleWithRemasteredWithParens()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.Album(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Album,
            Value = "Standing Hampton (REMASTERED)"
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
        Assert.Equal("Standing Hampton", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album)?.Value);
    }
    
    [Fact]
    public void ValidateAlbumTitleWithRemastered()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.Album(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Album,
            Value = "Standing Hampton Remastered"
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
        Assert.Equal("Standing Hampton", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album)?.Value);
    }    
    
    [Fact]
    public void ValidateAlbumTitleWithRemasteredWithDate()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.Album(NewConfiguration(), Serializer);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Album,
            Value = "Standing Hampton Remastered 2013"
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
        Assert.Equal("Standing Hampton", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album)?.Value);
    }
}
