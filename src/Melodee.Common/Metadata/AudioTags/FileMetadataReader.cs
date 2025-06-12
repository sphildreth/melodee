using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags;

public static class FileMetadataReader
{
    public static async Task<AudioFileMetadata> GetFileMetadataAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        // Simulate async for API consistency
        await Task.Yield();
        return new AudioFileMetadata
        {
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTimeUtc,
            Created = fileInfo.CreationTimeUtc,
            FilePath = filePath
        };
    }
}
