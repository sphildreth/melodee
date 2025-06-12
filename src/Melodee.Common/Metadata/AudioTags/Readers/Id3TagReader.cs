using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Readers;

public class Id3TagReader : ITagReader
{
    public async Task<IDictionary<MetaTagIdentifier, object>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tags = new Dictionary<MetaTagIdentifier, object>();
        const int id3v1TagSize = 128;
        const int id3v2HeaderSize = 10;
        byte[] buffer;

        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            // --- ID3v2 ---
            if (stream.Length >= id3v2HeaderSize)
            {
                buffer = new byte[id3v2HeaderSize];
                stream.Seek(0, SeekOrigin.Begin);
                var read = await stream.ReadAsync(buffer, 0, id3v2HeaderSize, cancellationToken);
                if (read == id3v2HeaderSize && buffer[0] == 'I' && buffer[1] == 'D' && buffer[2] == '3')
                {
                    int version = buffer[3];
                    int flags = buffer[5];
                    var tagSize = ((buffer[6] & 0x7F) << 21) | ((buffer[7] & 0x7F) << 14) | ((buffer[8] & 0x7F) << 7) | (buffer[9] & 0x7F);
                    var tagData = new byte[tagSize];
                    if (await stream.ReadAsync(tagData, 0, tagSize, cancellationToken) == tagSize)
                    {
                        var pos = 0;
                        while (pos + 10 <= tagData.Length)
                        {
                            var frameId = Encoding.ASCII.GetString(tagData, pos, 4);
                            var frameSize = version == 3
                                ? (tagData[pos + 4] << 24) | (tagData[pos + 5] << 16) | (tagData[pos + 6] << 8) | tagData[pos + 7]
                                : ((tagData[pos + 4] & 0x7F) << 21) | ((tagData[pos + 5] & 0x7F) << 14) | ((tagData[pos + 6] & 0x7F) << 7) | (tagData[pos + 7] & 0x7F);
                            if (frameSize <= 0 || pos + 10 + frameSize > tagData.Length)
                            {
                                break;
                            }

                            var frameData = tagData.Skip(pos + 10).Take(frameSize).ToArray();
                            var value = ReadId3v2TextFrame(frameData);
                            switch (frameId)
                            {
                                case "TT2":
                                case "TIT2": tags[MetaTagIdentifier.Title] = value; break;
                                case "TP1":
                                case "TPE1": tags[MetaTagIdentifier.Artist] = value; break;
                                case "TAL":
                                case "TALB": tags[MetaTagIdentifier.Album] = value; break;
                                case "TYE":
                                case "TYER":
                                case "TDRC": tags[MetaTagIdentifier.RecordingYear] = value; break;
                                case "TRK":
                                case "TRCK":
                                    if (int.TryParse(value.Split('/')[0], out var trackNum))
                                    {
                                        tags[MetaTagIdentifier.TrackNumber] = trackNum;
                                    }

                                    break;
                                case "TPA":
                                case "TPOS":
                                    if (int.TryParse(value.Split('/')[0], out var discNum))
                                    {
                                        tags[MetaTagIdentifier.DiscNumber] = discNum;
                                    }

                                    break;
                                case "TCO":
                                case "TCON": tags[MetaTagIdentifier.Genre] = value; break;
                                case "TCM":
                                case "TCOM": tags[MetaTagIdentifier.Composer] = value; break;
                                case "TCOP": tags[MetaTagIdentifier.Copyright] = value; break;
                                case "COMM": tags[MetaTagIdentifier.Comment] = value; break;
                            }

                            pos += 10 + frameSize;
                        }
                    }
                }
            }

            // --- ID3v1 ---
            if (stream.Length >= id3v1TagSize)
            {
                buffer = new byte[id3v1TagSize];
                stream.Seek(-id3v1TagSize, SeekOrigin.End);
                var read = await stream.ReadAsync(buffer, 0, id3v1TagSize, cancellationToken);
                if (read == id3v1TagSize && buffer[0] == 'T' && buffer[1] == 'A' && buffer[2] == 'G')
                {
                    tags[MetaTagIdentifier.Title] = Encoding.ASCII.GetString(buffer, 3, 30).TrimEnd('\0', ' ');
                    tags[MetaTagIdentifier.Artist] = Encoding.ASCII.GetString(buffer, 33, 30).TrimEnd('\0', ' ');
                    tags[MetaTagIdentifier.Album] = Encoding.ASCII.GetString(buffer, 63, 30).TrimEnd('\0', ' ');
                    tags[MetaTagIdentifier.RecordingYear] = Encoding.ASCII.GetString(buffer, 93, 4).TrimEnd('\0', ' ');
                    tags[MetaTagIdentifier.Comment] = Encoding.ASCII.GetString(buffer, 97, 28).TrimEnd('\0', ' ');
                    if (buffer[125] == 0 && buffer[126] != 0)
                    {
                        tags[MetaTagIdentifier.TrackNumber] = buffer[126];
                    }

                    tags[MetaTagIdentifier.Genre] = buffer[127];
                }
            }
        }

        return tags;
    }

    public async Task<object?> ReadTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default)
    {
        var tags = await ReadTagsAsync(filePath, cancellationToken);
        return tags.TryGetValue(tagId, out var value) ? value : null;
    }

    public Task<IReadOnlyList<AudioImage>> ReadImagesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // ID3v1 does not support images, and this implementation does not extract ID3v2 images
        return Task.FromResult<IReadOnlyList<AudioImage>>(new List<AudioImage>());
    }

    private static string ReadId3v2TextFrame(byte[] frameData)
    {
        if (frameData.Length == 0)
        {
            return string.Empty;
        }

        // First byte is encoding: 0=ISO-8859-1, 1=UTF-16, 2=UTF-16BE, 3=UTF-8
        switch (frameData[0])
        {
            case 0: return Encoding.GetEncoding("ISO-8859-1").GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
            case 1: return Encoding.Unicode.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
            case 2: return Encoding.BigEndianUnicode.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
            case 3: return Encoding.UTF8.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
            default: return Encoding.UTF8.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
        }
    }
}
