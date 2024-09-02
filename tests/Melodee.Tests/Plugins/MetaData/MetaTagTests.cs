using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.Processor;


namespace Melodee.Tests.Plugins.MetaData;

public class MetaTagTests
{
    [Fact]
    public async Task ValidateLoadingTagsForSimpleMp3FileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test.mp3";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag(new MetaTagsProcessor(TestsBase.NewConfiguration), TestsBase.NewConfiguration);
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            };
            var tagResult = await metaTag.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);
            Assert.NotNull(tagResult.Data.Tags);
            Assert.NotNull(tagResult.Data.File);
            Assert.Equal(fileInfo.FullName, tagResult.Data.File.FullName(dirInfo));
            Assert.NotNull(tagResult.Data.Title()?.Nullify());
        }
    }
    
    [Fact]
    public async Task ValidateLoadingTagsForMp3FileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test2.mp3";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            };
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag(new MetaTagsProcessor(TestsBase.NewConfiguration), TestsBase.NewConfiguration);
            var tagResult = await metaTag.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);

            var track = tagResult.Data;
            
            Assert.NotNull(track.Tags);
            Assert.NotNull(track.File);
            Assert.Equal(fileInfo.FullName, track.File.FullName(dirInfo));
            Assert.NotNull(track.Title()?.Nullify());
            Assert.False(track.TitleHasUnwantedText());
            Assert.True(track.Duration() > 0);
        }
    }
    
    [Fact]
    public async Task ValidateLoadingTagsForMp3Test4FileAsync()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/test4.mp3";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            };
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag(new MetaTagsProcessor(TestsBase.NewConfiguration), TestsBase.NewConfiguration);
            var tagResult = await metaTag.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);

            var track = tagResult.Data;
            
            Assert.NotNull(track.Tags);
            Assert.NotNull(track.File);
            Assert.Equal(fileInfo.FullName, track.File.FullName(dirInfo));
            Assert.NotNull(track.Title()?.Nullify());
            Assert.NotEmpty(track.ToTrackFileName());
        }
    }    

    [Fact]
    public async Task ValidateMultipleTrackArtistForMp3Async()
    {
        var testFile = @"/home/steven/incoming/melodee_test/tests/multipleTrackArtistsTest.mp3";
        var fileInfo = new System.IO.FileInfo(testFile);
        if (fileInfo.Exists)
        {
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = @"/home/steven/incoming/melodee_test/tests/",
                Name = "tests"
            };
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag(new MetaTagsProcessor(TestsBase.NewConfiguration), TestsBase.NewConfiguration);
            var tagResult = await metaTag.ProcessFileAsync(dirInfo, fileInfo.ToFileSystemInfo());
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);

            var track = tagResult.Data;
            
            Assert.NotNull(track.Tags);
            Assert.NotNull(track.File);
            var trackArtists = track.Tags!.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist);
            Assert.NotNull(trackArtists?.Value);

        }
    }
    
    


}
