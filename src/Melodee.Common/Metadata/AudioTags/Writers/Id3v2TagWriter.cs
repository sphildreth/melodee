using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Writers;

public class Id3v2TagWriter : ITagWriter
{
    public Task WriteTagsAsync(string filePath, IDictionary<MetaTagIdentifier, object> tags, CancellationToken cancellationToken = default)
    {
        // TODO: Implement writing/updating multiple ID3v2.4 tags
        return Task.CompletedTask;
    }

    public async Task WriteTagAsync(string filePath, MetaTagIdentifier tagId, object value, CancellationToken cancellationToken = default)
    {
        // Minimal implementation: only supports updating TIT2 (Title) for ID3v2.4
        // For a full implementation, all frames and sync-safe integer encoding should be handled
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, true);
        var header = new byte[10];
        if (await stream.ReadAsync(header, 0, 10, cancellationToken) != 10)
        {
            throw new IOException("File too small for ID3v2 header");
        }

        if (header[0] != 'I' || header[1] != 'D' || header[2] != '3')
        {
            throw new IOException("No ID3v2 tag found");
        }

        int version = header[3];
        var tagSize = ((header[6] & 0x7F) << 21) | ((header[7] & 0x7F) << 14) | ((header[8] & 0x7F) << 7) | (header[9] & 0x7F);
        var tagData = new byte[tagSize];
        if (await stream.ReadAsync(tagData, 0, tagSize, cancellationToken) != tagSize)
        {
            throw new IOException("Failed to read ID3v2 tag data");
        }

        // Find TIT2 frame
        var pos = 0;
        while (pos + 10 <= tagData.Length)
        {
            var frameId = Encoding.ASCII.GetString(tagData, pos, 4);
            var frameSize = ((tagData[pos + 4] & 0x7F) << 21) | ((tagData[pos + 5] & 0x7F) << 14) | ((tagData[pos + 6] & 0x7F) << 7) | (tagData[pos + 7] & 0x7F);
            if (frameId == "TIT2")
            {
                // Overwrite title
                var newTitle = Encoding.UTF8.GetBytes("\x03" + value);
                Array.Copy(newTitle, 0, tagData, pos + 10, Math.Min(newTitle.Length, frameSize));
                break;
            }

            pos += 10 + frameSize;
        }

        // Write back tagData
        stream.Seek(10, SeekOrigin.Begin);
        await stream.WriteAsync(tagData, 0, tagData.Length, cancellationToken);
    }

    public Task RemoveTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement removing a single ID3v2.4 tag
        return Task.CompletedTask;
    }

    public Task AddImageAsync(string filePath, AudioImage image, CancellationToken cancellationToken = default)
    {
        // TODO: Implement adding an image to ID3v2.4 tag
        return Task.CompletedTask;
    }

    public Task RemoveImagesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // TODO: Implement removing all images from ID3v2.4 tag
        return Task.CompletedTask;
    }
}
