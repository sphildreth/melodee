using Melodee.Common.Exceptions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;


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
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag();
            var tagResult = await metaTag.ProcessFileAsync(fileInfo);
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);
            Assert.NotNull(tagResult.Data.Tags);
            Assert.NotNull(tagResult.Data.FileSystemInfo);
            Assert.Equal(fileInfo.FullName, tagResult.Data.FileSystemInfo.FullName);
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
            var metaTag = new Melodee.Plugins.MetaData.Track.MetaTag();
            var tagResult = await metaTag.ProcessFileAsync(fileInfo);
            Assert.NotNull(tagResult);
            Assert.True(tagResult.IsSuccess);
            Assert.NotNull(tagResult.Data);
            Assert.NotNull(tagResult.Data.Tags);
            Assert.NotNull(tagResult.Data.FileSystemInfo);
            Assert.Equal(fileInfo.FullName, tagResult.Data.FileSystemInfo.FullName);
            Assert.NotNull(tagResult.Data.Title()?.Nullify());
        }
    }    
}