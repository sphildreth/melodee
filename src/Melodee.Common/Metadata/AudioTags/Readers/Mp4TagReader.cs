using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Readers;

public class Mp4TagReader : ITagReader
{
    public async Task<IDictionary<MetaTagIdentifier, object>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tags = new Dictionary<MetaTagIdentifier, object>();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        // MP4 atoms are in a tree structure. We'll scan for 'moov' > 'udta' > 'meta' > 'ilst' atoms and parse common tags.
        // This is a minimal implementation for common iTunes tags.
        // For brevity, this implementation is simplified and may not cover all edge cases.
        var fileLength = stream.Length;
        long pos = 0;
        while (pos + 8 < fileLength)
        {
            stream.Seek(pos, SeekOrigin.Begin);
            var atomHeader = new byte[8];
            if (await stream.ReadAsync(atomHeader, 0, 8, cancellationToken) != 8)
            {
                break;
            }

            var atomSize = (atomHeader[0] << 24) | (atomHeader[1] << 16) | (atomHeader[2] << 8) | atomHeader[3];
            var atomType = Encoding.ASCII.GetString(atomHeader, 4, 4);
            if (atomType == "moov" || atomType == "udta" || atomType == "meta" || atomType == "ilst")
            {
                pos += 8;
                continue;
            }

            if (atomType == "©nam") // Title
            {
                tags[MetaTagIdentifier.Title] = await ReadMp4StringAtom(stream, atomSize - 8, cancellationToken);
            }
            else if (atomType == "©ART") // Artist
            {
                tags[MetaTagIdentifier.Artist] = await ReadMp4StringAtom(stream, atomSize - 8, cancellationToken);
            }
            else if (atomType == "©alb") // Album
            {
                tags[MetaTagIdentifier.Album] = await ReadMp4StringAtom(stream, atomSize - 8, cancellationToken);
            }
            else if (atomType == "©day") // Year/Date
            {
                tags[MetaTagIdentifier.RecordingDateOrYear] = await ReadMp4StringAtom(stream, atomSize - 8, cancellationToken);
            }
            else if (atomType == "©gen") // Genre
            {
                tags[MetaTagIdentifier.Genre] = await ReadMp4StringAtom(stream, atomSize - 8, cancellationToken);
            }
            else if (atomType == "covr") // Cover art
            {
                // Handled in ReadImagesAsync
            }

            pos += atomSize > 0 ? atomSize : 8;
        }

        return tags;
    }

    public async Task<IReadOnlyList<AudioImage>> ReadImagesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var images = new List<AudioImage>();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var fileLength = stream.Length;
        long pos = 0;
        while (pos + 8 < fileLength)
        {
            stream.Seek(pos, SeekOrigin.Begin);
            var atomHeader = new byte[8];
            if (await stream.ReadAsync(atomHeader, 0, 8, cancellationToken) != 8)
            {
                break;
            }

            var atomSize = (atomHeader[0] << 24) | (atomHeader[1] << 16) | (atomHeader[2] << 8) | atomHeader[3];
            var atomType = Encoding.ASCII.GetString(atomHeader, 4, 4);
            if (atomType == "covr")
            {
                var data = new byte[atomSize - 8];
                if (await stream.ReadAsync(data, 0, atomSize - 8, cancellationToken) == atomSize - 8)
                {
                    // MP4 cover art is usually JPEG or PNG, with a 16-byte data header
                    var imgStart = 16;
                    if (data.Length > imgStart)
                    {
                        var imgData = new byte[data.Length - imgStart];
                        Array.Copy(data, imgStart, imgData, 0, imgData.Length);
                        images.Add(new AudioImage
                        {
                            Data = imgData,
                            MimeType = "image/jpeg", // Could be PNG, but most are JPEG
                            Description = null,
                            Type = PictureIdentifier.Front
                        });
                    }
                }
            }

            pos += atomSize > 0 ? atomSize : 8;
        }

        return images;
    }

    public async Task<object?> ReadTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default)
    {
        var tags = await ReadTagsAsync(filePath, cancellationToken);
        return tags.TryGetValue(tagId, out var value) ? value : null;
    }

    private static async Task<string> ReadMp4StringAtom(FileStream stream, int size, CancellationToken cancellationToken)
    {
        var data = new byte[size];
        if (await stream.ReadAsync(data, 0, size, cancellationToken) != size)
        {
            return string.Empty;
        }

        // MP4 string atoms have a data header (usually 16 bytes), then the string
        var strStart = 16;
        if (size <= strStart)
        {
            return string.Empty;
        }

        return Encoding.UTF8.GetString(data, strStart, size - strStart).TrimEnd('\0', ' ');
    }
}
