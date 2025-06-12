using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Readers;

public class ApeTagReader : ITagReader
{
    // Constants for APE tag format
    private const string ApeTagId = "APETAGEX";
    private const int ApeTagFooterSize = 32;
    private const uint ApeFlagHeaderPresent = 0x80000000;
    private const int TagItemFlagIsText = 0;
    private const int TagItemFlagIsUtf8 = 1 << 0;
    private const int TagItemFlagIsBinary = 1 << 1;

    public async Task<IDictionary<MetaTagIdentifier, object>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tags = new Dictionary<MetaTagIdentifier, object>();
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

        // Check if file is large enough to contain a tag
        if (stream.Length < ApeTagFooterSize)
        {
            return tags;
        }

        // Read the footer
        var footer = new byte[ApeTagFooterSize];
        stream.Seek(-ApeTagFooterSize, SeekOrigin.End);
        var readFooter = await stream.ReadAsync(footer, 0, ApeTagFooterSize, cancellationToken);
        if (readFooter != ApeTagFooterSize || Encoding.ASCII.GetString(footer, 0, 8) != ApeTagId)
        {
            return tags;
        }

        // Parse footer
        var tagSize = BitConverter.ToInt32(footer, 12);
        var itemCount = BitConverter.ToInt32(footer, 16);
        var flags = BitConverter.ToUInt32(footer, 20);
        if (tagSize < ApeTagFooterSize || itemCount <= 0 || tagSize > stream.Length)
        {
            return tags;
        }

        // Seek to start of tag (may have header)
        var tagStart = stream.Length - tagSize;
        stream.Seek(tagStart, SeekOrigin.Begin);
        var tagData = new byte[tagSize];
        var readTag = await stream.ReadAsync(tagData, 0, tagSize, cancellationToken);
        if (readTag != tagSize)
        {
            return tags;
        }

        // If header present, skip it
        var pos = 0;
        if ((flags & ApeFlagHeaderPresent) != 0 && Encoding.ASCII.GetString(tagData, 0, 8) == ApeTagId)
        {
            pos += ApeTagFooterSize;
        }

        var images = new List<AudioImage>();
        for (var i = 0; i < itemCount && pos + 8 < tagData.Length; i++)
        {
            var valueSize = BitConverter.ToInt32(tagData, pos);
            var itemFlags = BitConverter.ToInt32(tagData, pos + 4);
            var keyStart = pos + 8;
            var keyEnd = Array.IndexOf(tagData, (byte)0, keyStart);
            if (keyEnd < 0 || keyEnd >= tagData.Length)
            {
                break;
            }

            var key = Encoding.UTF8.GetString(tagData, keyStart, keyEnd - keyStart);
            var valueStart = keyEnd + 1;
            if (valueStart + valueSize > tagData.Length)
            {
                break;
            }

            if ((itemFlags & TagItemFlagIsBinary) != 0)
            {
                if (key.Equals("Cover Art (Front)", StringComparison.OrdinalIgnoreCase))
                {
                    var nullSep = Array.IndexOf(tagData, (byte)0, valueStart);
                    if (nullSep > valueStart && nullSep < valueStart + valueSize)
                    {
                        var desc = Encoding.UTF8.GetString(tagData, valueStart, nullSep - valueStart);
                        var imgOffset = nullSep + 1;
                        var imgSize = valueSize - (nullSep - valueStart + 1);
                        if (imgSize > 0)
                        {
                            var imgData = new byte[imgSize];
                            Array.Copy(tagData, imgOffset, imgData, 0, imgSize);
                            images.Add(new AudioImage { Description = desc, Data = imgData, Type = PictureIdentifier.Front });
                        }
                    }
                }
            }
            else
            {
                var value = Encoding.UTF8.GetString(tagData, valueStart, valueSize).TrimEnd('\0', ' ');
                switch (key.ToUpperInvariant())
                {
                    case "TITLE": tags[MetaTagIdentifier.Title] = value; break;
                    case "ARTIST": tags[MetaTagIdentifier.Artist] = value; break;
                    case "ALBUM": tags[MetaTagIdentifier.Album] = value; break;
                    case "ALBUM ARTIST":
                    case "ALBUMARTIST": tags[MetaTagIdentifier.AlbumArtist] = value; break;
                    case "YEAR": tags[MetaTagIdentifier.RecordingYear] = value; break;
                    case "GENRE": tags[MetaTagIdentifier.Genre] = value; break;
                    case "COMMENT": tags[MetaTagIdentifier.Comment] = value; break;
                    case "TRACK":
                    case "TRACKNUMBER":
                        if (int.TryParse(value.Split('/')[0], out var trackNum))
                        {
                            tags[MetaTagIdentifier.TrackNumber] = trackNum;
                        }

                        break;
                    case "DISC":
                    case "DISCNUMBER":
                        if (int.TryParse(value.Split('/')[0], out var discNum))
                        {
                            tags[MetaTagIdentifier.DiscNumber] = discNum;
                        }

                        break;
                    case "COMPOSER": tags[MetaTagIdentifier.Composer] = value; break;
                    case "COPYRIGHT": tags[MetaTagIdentifier.Copyright] = value; break;
                    case "COMPILATION":
                        if (bool.TryParse(value, out var isComp))
                        {
                            tags[MetaTagIdentifier.Compilation] = isComp;
                        }

                        break;
                    case "BPM":
                        if (int.TryParse(value, out var bpm))
                        {
                            tags[MetaTagIdentifier.Bpm] = bpm;
                        }

                        break;
                    case "REPLAYGAIN_TRACK_GAIN": tags[MetaTagIdentifier.ReplayGainTrack] = value; break;
                    case "REPLAYGAIN_ALBUM_GAIN": tags[MetaTagIdentifier.ReplayGainAlbum] = value; break;
                }
            }

            pos = valueStart + valueSize;
        }

        if (images.Count > 0)
        {
            tags[MetaTagIdentifier.Images] = images;
        }

        return tags;
    }

    public async Task<object?> ReadTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default)
    {
        var tags = await ReadTagsAsync(filePath, cancellationToken);
        return tags.TryGetValue(tagId, out var value) ? value : null;
    }

    public async Task<IReadOnlyList<AudioImage>> ReadImagesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tags = await ReadTagsAsync(filePath, cancellationToken);

        if (tags.TryGetValue(MetaTagIdentifier.Images, out var imagesObj) && imagesObj is List<AudioImage> images)
        {
            return images;
        }

        return new List<AudioImage>();
    }
}
