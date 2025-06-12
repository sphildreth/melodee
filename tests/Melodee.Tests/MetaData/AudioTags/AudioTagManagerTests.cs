using System.Threading.Tasks;
using Xunit;
using Melodee.Common.Metadata.AudioTags;
using System.IO;
using System.Threading;
using Melodee.Common.Utility;

namespace Melodee.Tests.MetaData.AudioTags
{
    public class AudioTagManagerTests
    {
        [Fact]
        public async Task Throws_On_Unknown_Format()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                await Assert.ThrowsAsync<System.NotSupportedException>(async () =>
                {
                    await AudioTagManager.ReadAllTagsAsync(tempFile, CancellationToken.None);
                });
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task Read_All_Media_Files_In_Folder()
        {
            var testFolder = Path.Combine(Directory.GetCurrentDirectory(), "/melodee_test/tests/good");
            if (!Directory.Exists(testFolder))
            {
                return;
            }
            foreach (var file in await AudioTagManager.AllMediaFilesForDirectoryAsync(testFolder))
            {
                var tags = await AudioTagManager.ReadAllTagsAsync(file.FullName, CancellationToken.None);
                Assert.NotEqual(AudioFormat.Unknown, tags.Format);
                Assert.NotEqual(0, tags.FileMetadata.FileSize);
                Assert.NotEqual(string.Empty, tags.FileMetadata.FilePath);
                Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.Created);
                Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.LastModified);
                Assert.NotNull(tags.Tags);
                Assert.NotEmpty(tags.Tags);
                Assert.NotEmpty(tags.Tags.Keys);
                Assert.NotEmpty(tags.Tags.Values);
                Assert.NotEmpty(SafeParser.ToString(tags.Tags[Melodee.Common.Enums.MetaTagIdentifier.Artist]));
                Assert.True(SafeParser.ToNumber<int>(tags.Tags[Melodee.Common.Enums.MetaTagIdentifier.TrackNumber]) > 0);
                Assert.NotEmpty(SafeParser.ToString(tags.Tags[Melodee.Common.Enums.MetaTagIdentifier.Title]));
            }
            
        }
    }
}
