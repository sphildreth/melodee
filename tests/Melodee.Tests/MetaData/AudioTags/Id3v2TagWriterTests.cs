using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Writers;

namespace Melodee.Tests.MetaData.AudioTags;

public class Id3v2TagWriterTests
{
    [Fact]
    public async Task Throws_On_NonTagged_File()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var writer = new Id3v2TagWriter();
            await Assert.ThrowsAsync<IOException>(async () => { await writer.WriteTagAsync(tempFile, MetaTagIdentifier.Title, "Test", CancellationToken.None); });
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
