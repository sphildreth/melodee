using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags;
using Melodee.Common.Metadata.AudioTags.Readers;
using Melodee.Common.Utility;

namespace Melodee.Tests.MetaData.AudioTags;

public class VorbisTagReaderTests
{
    [Fact]
    public async Task Returns_Empty_On_NonTagged_File()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var reader = new VorbisTagReader();
            var tags = await reader.ReadTagsAsync(tempFile, CancellationToken.None);
            Assert.Empty(tags);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Read_Media_Files_In_Test_Folder()
    {
        var testFolder = "/melodee_test/tests/good";
        var testFile = "test.ogg";
        if (!Directory.Exists(testFolder))
        {
            return;
        }

        var tags = await AudioTagManager.ReadAllTagsAsync(Path.Combine(testFolder, testFile), CancellationToken.None);
        Assert.NotEqual(AudioFormat.Unknown, tags.Format);
        Assert.NotEqual(0, tags.FileMetadata.FileSize);
        Assert.NotEqual(string.Empty, tags.FileMetadata.FilePath);
        Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.Created);
        Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.LastModified);
        Assert.NotNull(tags.Tags);
        Assert.NotEmpty(tags.Tags);
        Assert.NotEmpty(tags.Tags.Keys);
        Assert.NotEmpty(tags.Tags.Values);
        Assert.NotEmpty(SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
        Assert.True(SafeParser.ToNumber<int>(tags.Tags[MetaTagIdentifier.TrackNumber]) > 0);
        Assert.NotEmpty(SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
    }
}
