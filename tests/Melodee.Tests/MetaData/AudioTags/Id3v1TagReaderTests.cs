using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags;
using Melodee.Common.Utility;

namespace Melodee.Tests.MetaData.AudioTags;

public class Id3v1TagReaderTests
{
    [Fact]
    public async Task Read_Media_Files_In_Test_Folder()
    {
        var testFolder = "/melodee_test/tests/good";
        if (!Directory.Exists(testFolder))
        {
            return;
        }

        var tags = await AudioTagManager.ReadAllTagsAsync(Path.Combine(testFolder, "test_3_1.mp3"), CancellationToken.None);

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
