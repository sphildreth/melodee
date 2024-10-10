using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Tests.Plugins.Processors.MetaTagProcessors;

public class TrackTitleMetaTagProcessorTests
{
    [Fact]
    public void ValidateTrackTitleWithFeaturing()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.TrackTitle(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Some name (ft Nonna)"
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
        Assert.Equal("Some Name", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }
    
    [Fact]
    public void ValidateTrackTitleWithWithShouldRemove1()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.TrackTitle(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Fly Way (with Bob Jones)"
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
        Assert.Equal("Fly Way", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }
    
    [Fact]
    public void ValidateTrackTitleWithWithShouldRemove2()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.TrackTitle(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "Fly Way with Bob Jones"
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
        Assert.Equal("Fly Way", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }    
    
    [Fact]
    public void ValidateTrackTitleWithWithShouldStay()
    {
        var metatagProcessor = new Melodee.Plugins.Processor.MetaTagProcessors.TrackTitle(TestsBase.NewConfiguration);
        var tag = new MetaTag<object?>
        {
            Identifier = MetaTagIdentifier.Title,
            Value = "With Me (Radio Edit)"
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
        Assert.Equal("With Me (Radio Edit)", result.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title)?.Value);
    }    
}
