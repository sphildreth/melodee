using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Readers;

public class Id3TagReader : ITagReader
{
    // Maximum frame size - 16MB should be plenty for any realistic tag
    private const int MaxFrameSize = 16 * 1024 * 1024;

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
                try
                {
                    buffer = new byte[id3v2HeaderSize];
                    stream.Seek(0, SeekOrigin.Begin);
                    var read = await stream.ReadAsync(buffer, 0, id3v2HeaderSize, cancellationToken);
                    if (read == id3v2HeaderSize && buffer[0] == 'I' && buffer[1] == 'D' && buffer[2] == '3')
                    {
                        int majorVersion = buffer[3];
                        // int minorVersion = buffer[4]; // Not used currently
                        int flags = buffer[5];

                        // Calculate size from syncsafe integer
                        var tagSize = ((buffer[6] & 0x7F) << 21) |
                                      ((buffer[7] & 0x7F) << 14) |
                                      ((buffer[8] & 0x7F) << 7) |
                                      (buffer[9] & 0x7F);

                        // Sanity check: 100 MB maximum to prevent out-of-memory with corrupt headers
                        if (tagSize > 0 && tagSize < 100 * 1024 * 1024)
                        {
                            var tagData = new byte[tagSize];
                            var totalBytesRead = 0;
                            int bytesRead;

                            // Read in chunks to handle large tags more efficiently
                            while (totalBytesRead < tagSize &&
                                   (bytesRead = await stream.ReadAsync(
                                       tagData,
                                       totalBytesRead,
                                       Math.Min(4096, tagSize - totalBytesRead),
                                       cancellationToken)) > 0)
                            {
                                totalBytesRead += bytesRead;

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }
                            }

                            if (totalBytesRead == tagSize)
                            {
                                ProcessId3v2Frames(tagData, majorVersion, flags, tags);
                            }
                            else
                            {
                                Console.WriteLine($"Warning: Incomplete ID3v2 tag read ({totalBytesRead} of {tagSize} bytes)");
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Log error but continue to ID3v1 if present
                    // This makes the reader more robust against malformed tags
                    Console.WriteLine($"Error reading ID3v2 tag: {ex.Message}");
                }
            }

            // --- ID3v1 ---
            if (stream.Length >= id3v1TagSize)
            {
                try
                {
                    buffer = new byte[id3v1TagSize];
                    stream.Seek(-id3v1TagSize, SeekOrigin.End);
                    var read = await stream.ReadAsync(buffer, 0, id3v1TagSize, cancellationToken);
                    if (read == id3v1TagSize && buffer[0] == 'T' && buffer[1] == 'A' && buffer[2] == 'G')
                    {
                        // Only add ID3v1 tags if we don't already have them from ID3v2
                        if (!tags.ContainsKey(MetaTagIdentifier.Title) || string.IsNullOrEmpty(tags[MetaTagIdentifier.Title]?.ToString()))
                        {
                            tags[MetaTagIdentifier.Title] = Encoding.ASCII.GetString(buffer, 3, 30).TrimEnd('\0', ' ');
                        }

                        if (!tags.ContainsKey(MetaTagIdentifier.Artist) || string.IsNullOrEmpty(tags[MetaTagIdentifier.Artist]?.ToString()))
                        {
                            tags[MetaTagIdentifier.Artist] = Encoding.ASCII.GetString(buffer, 33, 30).TrimEnd('\0', ' ');
                        }

                        if (!tags.ContainsKey(MetaTagIdentifier.Album) || string.IsNullOrEmpty(tags[MetaTagIdentifier.Album]?.ToString()))
                        {
                            tags[MetaTagIdentifier.Album] = Encoding.ASCII.GetString(buffer, 63, 30).TrimEnd('\0', ' ');
                        }

                        if (!tags.ContainsKey(MetaTagIdentifier.RecordingYear) || string.IsNullOrEmpty(tags[MetaTagIdentifier.RecordingYear]?.ToString()))
                        {
                            tags[MetaTagIdentifier.RecordingYear] = Encoding.ASCII.GetString(buffer, 93, 4).TrimEnd('\0', ' ');
                        }

                        if (!tags.ContainsKey(MetaTagIdentifier.Comment) || string.IsNullOrEmpty(tags[MetaTagIdentifier.Comment]?.ToString()))
                        {
                            // Check for ID3v1.1 format (track number in byte 125)
                            if (buffer[125] == 0 && buffer[126] != 0)
                            {
                                tags[MetaTagIdentifier.Comment] = Encoding.ASCII.GetString(buffer, 97, 28).TrimEnd('\0', ' ');

                                if (!tags.ContainsKey(MetaTagIdentifier.TrackNumber) ||
                                    string.IsNullOrEmpty(tags[MetaTagIdentifier.TrackNumber]?.ToString()) ||
                                    Convert.ToInt32(tags[MetaTagIdentifier.TrackNumber]) == 0)
                                {
                                    tags[MetaTagIdentifier.TrackNumber] = buffer[126].ToString();
                                }
                            }
                            else
                            {
                                // ID3v1.0 format (no track number)
                                tags[MetaTagIdentifier.Comment] = Encoding.ASCII.GetString(buffer, 97, 30).TrimEnd('\0', ' ');
                            }
                        }

                        if (!tags.ContainsKey(MetaTagIdentifier.Genre))
                        {
                            tags[MetaTagIdentifier.Genre] = GetGenreFromCode(buffer[127]);
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Log error but continue
                    Console.WriteLine($"Error reading ID3v1 tag: {ex.Message}");
                }
            }
        }

        // Always ensure we have empty strings rather than nulls or missing keys
        EnsureEmptyStringDefaults(tags);

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
        var images = new List<AudioImage>();
        if (tags.TryGetValue(MetaTagIdentifier.CoverArt, out var coverArtObj) && coverArtObj is byte[] coverArtBytes && coverArtBytes.Length > 0)
        {
            images.Add(new AudioImage
            {
                Data = coverArtBytes
                // Optionally set MimeType, Description, etc. if needed
            });
        }

        return images;
    }

    private void ProcessId3v2Frames(byte[] tagData, int majorVersion, int flags, Dictionary<MetaTagIdentifier, object> tags)
    {
        var pos = 0;
        var frameHeaderSize = majorVersion >= 3 ? 10 : 6; // ID3v2.3+ uses 10-byte frame headers, ID3v2.2 uses 6-byte

        while (pos + frameHeaderSize <= tagData.Length)
        {
            // Check if we've reached padding (zeros)
            if (tagData[pos] == 0)
            {
                break;
            }

            string frameId;
            int frameSize;

            // Check if we have enough data left for a valid frame
            if (pos + frameHeaderSize > tagData.Length)
            {
                break;
            }

            if (majorVersion >= 3) // ID3v2.3 or ID3v2.4
            {
                // Make sure the frame ID is valid ASCII
                if (!IsValidFrameId(tagData, pos, 4))
                {
                    break;
                }

                frameId = Encoding.ASCII.GetString(tagData, pos, 4);

                // Calculate frame size depending on ID3v2 version
                if (majorVersion == 3) // ID3v2.3 - not a syncsafe integer
                {
                    frameSize = (tagData[pos + 4] << 24) |
                                (tagData[pos + 5] << 16) |
                                (tagData[pos + 6] << 8) |
                                tagData[pos + 7];
                }
                else // ID3v2.4 - syncsafe integer
                {
                    frameSize = ((tagData[pos + 4] & 0x7F) << 21) |
                                ((tagData[pos + 5] & 0x7F) << 14) |
                                ((tagData[pos + 6] & 0x7F) << 7) |
                                (tagData[pos + 7] & 0x7F);
                }
            }
            else // ID3v2.2
            {
                // Make sure the frame ID is valid ASCII
                if (!IsValidFrameId(tagData, pos, 3))
                {
                    break;
                }

                frameId = Encoding.ASCII.GetString(tagData, pos, 3);
                frameSize = (tagData[pos + 3] << 16) |
                            (tagData[pos + 4] << 8) |
                            tagData[pos + 5];

                // Map ID3v2.2 frame IDs to their ID3v2.3 equivalents for consistent processing
                switch (frameId)
                {
                    case "TT2": frameId = "TIT2"; break;
                    case "TP1": frameId = "TPE1"; break;
                    case "TAL": frameId = "TALB"; break;
                    case "TYE": frameId = "TYER"; break;
                    case "TRK": frameId = "TRCK"; break;
                    case "TPA": frameId = "TPOS"; break;
                    case "TCO": frameId = "TCON"; break;
                    case "TCM": frameId = "TCOM"; break;
                    case "COM": frameId = "COMM"; break;
                }
            }

            // Sanity check frameSize:
            // 1. Must be positive
            // 2. Must not exceed MaxFrameSize (16MB)
            // 3. Must not go beyond the tag data
            if (frameSize <= 0 || frameSize > MaxFrameSize || pos + frameHeaderSize + frameSize > tagData.Length)
            {
                // Skip to the next potential frame
                pos++;
                continue;
            }

            try
            {
                var frameData = new byte[frameSize];
                Array.Copy(tagData, pos + frameHeaderSize, frameData, 0, frameSize);

                // Special handling for COMM frames as they have a different structure
                if (frameId == "COMM")
                {
                    ProcessCommentFrame(frameData, tags);
                }
                else if (frameId == "APIC")
                {
                    // Handle album art (APIC) frame
                    try
                    {
                        // First byte is text encoding
                        int encoding = frameData[0];
                        var offset = 1;

                        // Find the null terminator for MIME type (always ASCII regardless of encoding)
                        var mimeEnd = Array.IndexOf(frameData, (byte)0, offset);
                        if (mimeEnd == -1)
                        {
                            mimeEnd = frameData.Length - 1;
                        }

                        // Skip past MIME type null terminator
                        offset = mimeEnd + 1;
                        if (offset >= frameData.Length)
                        {
                            throw new Exception("APIC frame truncated after MIME type");
                        }

                        // Picture type (1 byte)
                        var pictureType = frameData[offset];
                        offset++;
                        if (offset >= frameData.Length)
                        {
                            throw new Exception("APIC frame truncated after picture type");
                        }

                        // Find the null terminator for the description (depends on encoding)
                        var descEndOffset = offset;
                        if (encoding == 0 || encoding == 3) // ISO-8859-1 or UTF-8: 1-byte null
                        {
                            // Find next null byte
                            var nullPos = Array.IndexOf(frameData, (byte)0, offset);
                            if (nullPos == -1)
                            {
                                nullPos = frameData.Length;
                            }

                            descEndOffset = nullPos + 1; // Skip past this null terminator
                        }
                        else // UTF-16 (encoding == 1 or 2): 2-byte null terminator
                        {
                            // Find the double-null terminator (both bytes are zero)
                            var foundTerminator = false;
                            while (descEndOffset + 1 < frameData.Length && !foundTerminator)
                            {
                                if (frameData[descEndOffset] == 0 && frameData[descEndOffset + 1] == 0)
                                {
                                    foundTerminator = true;
                                    descEndOffset += 2; // Skip past the 2-byte terminator
                                }
                                else
                                {
                                    descEndOffset += 2; // Skip current character (2 bytes)
                                }
                            }

                            if (!foundTerminator)
                            {
                                // If we didn't find a proper terminator, just use the rest of the data
                                descEndOffset = frameData.Length;
                            }
                        }

                        // Ensure we don't go out of bounds
                        if (descEndOffset > frameData.Length)
                        {
                            descEndOffset = frameData.Length;
                        }

                        // The rest is the image data
                        var imageDataLen = frameData.Length - descEndOffset;
                        if (imageDataLen > 0)
                        {
                            var imageData = new byte[imageDataLen];
                            Array.Copy(frameData, descEndOffset, imageData, 0, imageDataLen);

                            // Store the image data under both tag keys
                            tags[MetaTagIdentifier.CoverArt] = imageData;
                            tags[MetaTagIdentifier.AlbumArt] = imageData;

                            // Also store as an AudioImage list
                            tags[MetaTagIdentifier.Images] = new List<AudioImage> { new() { Data = imageData } };
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing APIC frame: {ex.Message}");
                    }
                }
                else if (frameId.StartsWith("T") && frameId != "TXXX") // Standard text frame
                {
                    var value = ReadId3v2TextFrame(frameData);

                    // Map frame ID to MetaTagIdentifier and assign value
                    switch (frameId)
                    {
                        case "TIT2": tags[MetaTagIdentifier.Title] = value; break;
                        case "TPE1": tags[MetaTagIdentifier.Artist] = value; break;
                        case "TPE2": tags[MetaTagIdentifier.AlbumArtist] = value; break;
                        case "TALB": tags[MetaTagIdentifier.Album] = value; break;
                        case "TYER": // v2.3
                        case "TDRC": // v2.4
                            tags[MetaTagIdentifier.RecordingYear] = value;
                            break;
                        case "TRCK":
                            var trackPart = value.Split('/')[0];
                            tags[MetaTagIdentifier.TrackNumber] = trackPart; // Store as string to preserve leading zeros
                            break;
                        case "TPOS":
                            var discPart = value.Split('/')[0];
                            tags[MetaTagIdentifier.DiscNumber] = discPart;
                            break;
                        case "TCON":
                            // Handle potential genre format like "(17)Rock"
                            tags[MetaTagIdentifier.Genre] = ParseGenre(value);
                            break;
                        case "TCOM": tags[MetaTagIdentifier.Composer] = value; break;
                        case "TCOP": tags[MetaTagIdentifier.Copyright] = value; break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue processing other frames
                Console.WriteLine($"Error processing frame {frameId}: {ex.Message}");
            }

            // Move to the next frame
            pos += frameHeaderSize + frameSize;
        }
    }

    private void ProcessCommentFrame(byte[] frameData, Dictionary<MetaTagIdentifier, object> tags)
    {
        if (frameData == null || frameData.Length < 5) // Need at least encoding byte + language (3 bytes) + 1 byte
        {
            return;
        }

        try
        {
            // First byte is encoding
            var encoding = frameData[0];

            // Next 3 bytes are language
            var language = Encoding.ASCII.GetString(frameData, 1, 3);

            // After language comes the content description (short), terminated by 00 or 00 00 depending on encoding
            // Then comes the actual comment text
            string comment;

            switch (encoding)
            {
                case 0: // ISO-8859-1
                    // Find the first null byte after the language field
                    var descEndPos = Array.IndexOf(frameData, (byte)0, 4);
                    if (descEndPos == -1)
                    {
                        descEndPos = frameData.Length; // No terminator found
                    }

                    // Actual comment starts after the terminator
                    var commentStart = descEndPos + 1;
                    if (commentStart < frameData.Length)
                    {
                        comment = Encoding.GetEncoding("ISO-8859-1").GetString(
                            frameData, commentStart, frameData.Length - commentStart).TrimEnd('\0');
                        tags[MetaTagIdentifier.Comment] = comment;
                    }

                    break;

                case 1: // UTF-16 with BOM
                    // UTF-16 uses 2-byte null terminator
                    var pos = 4;
                    while (pos + 1 < frameData.Length)
                    {
                        if (frameData[pos] == 0 && frameData[pos + 1] == 0)
                        {
                            // Found description terminator, now read comment text
                            pos += 2;
                            if (pos < frameData.Length)
                            {
                                // Check if there's a BOM (Byte Order Mark)
                                var hasBom = pos + 1 < frameData.Length &&
                                             ((frameData[pos] == 0xFF && frameData[pos + 1] == 0xFE) ||
                                              (frameData[pos] == 0xFE && frameData[pos + 1] == 0xFF));

                                var startPos = hasBom ? pos + 2 : pos;
                                var length = frameData.Length - startPos;

                                if (length > 0)
                                {
                                    var isBigEndian = hasBom && frameData[pos] == 0xFE && frameData[pos + 1] == 0xFF;
                                    var encoding16 = isBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;

                                    comment = encoding16.GetString(frameData, startPos, length).TrimEnd('\0');
                                    tags[MetaTagIdentifier.Comment] = comment;
                                }
                            }

                            break;
                        }

                        pos += 2;
                    }

                    break;

                case 2: // UTF-16BE (no BOM)
                    // Find the description terminator (2 bytes of zeros)
                    pos = 4;
                    while (pos + 1 < frameData.Length)
                    {
                        if (frameData[pos] == 0 && frameData[pos + 1] == 0)
                        {
                            // Found description terminator
                            pos += 2;
                            if (pos < frameData.Length)
                            {
                                comment = Encoding.BigEndianUnicode.GetString(
                                    frameData, pos, frameData.Length - pos).TrimEnd('\0');
                                tags[MetaTagIdentifier.Comment] = comment;
                            }

                            break;
                        }

                        pos += 2;
                    }

                    break;

                case 3: // UTF-8
                    // Find the first null byte after the language field
                    descEndPos = Array.IndexOf(frameData, (byte)0, 4);
                    if (descEndPos == -1)
                    {
                        descEndPos = frameData.Length; // No terminator found
                    }

                    // Actual comment starts after the terminator
                    commentStart = descEndPos + 1;
                    if (commentStart < frameData.Length)
                    {
                        comment = Encoding.UTF8.GetString(
                            frameData, commentStart, frameData.Length - commentStart).TrimEnd('\0');
                        tags[MetaTagIdentifier.Comment] = comment;
                    }

                    break;

                default:
                    // Just get the rest of the data after language field, assume it's the comment
                    comment = Encoding.UTF8.GetString(frameData, 4, frameData.Length - 4).TrimEnd('\0');
                    tags[MetaTagIdentifier.Comment] = comment;
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing comment frame: {ex.Message}");
        }
    }

    private static string ReadId3v2TextFrame(byte[] frameData)
    {
        if (frameData == null || frameData.Length == 0)
        {
            return string.Empty;
        }

        // First byte is encoding: 0=ISO-8859-1, 1=UTF-16, 2=UTF-16BE, 3=UTF-8
        try
        {
            // Handle empty or unreasonably large frames
            if (frameData.Length <= 1 || frameData.Length > MaxFrameSize)
            {
                return string.Empty;
            }

            switch (frameData[0])
            {
                case 0: // ISO-8859-1
                    return Encoding.GetEncoding("ISO-8859-1").GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');

                case 1: // UTF-16 with BOM
                    if (frameData.Length < 3)
                    {
                        return string.Empty; // Need BOM + at least one character
                    }

                    // Check for BOM to determine endianness
                    var offset = 1;
                    var hasBom = frameData.Length > 3 &&
                                 ((frameData[1] == 0xFF && frameData[2] == 0xFE) ||
                                  (frameData[1] == 0xFE && frameData[2] == 0xFF));

                    if (hasBom)
                    {
                        offset = 3; // Skip BOM
                        var isBigEndian = frameData[1] == 0xFE && frameData[2] == 0xFF;
                        return (isBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode)
                            .GetString(frameData, offset, frameData.Length - offset).TrimEnd('\0', ' ');
                    }

                    // No BOM, assume little-endian (more common)
                    return Encoding.Unicode.GetString(frameData, offset, frameData.Length - offset).TrimEnd('\0', ' ');

                case 2: // UTF-16BE (no BOM)
                    return Encoding.BigEndianUnicode.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');

                case 3: // UTF-8
                    return Encoding.UTF8.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');

                default:
                    // Try UTF-8 as a fallback
                    return Encoding.UTF8.GetString(frameData, 1, frameData.Length - 1).TrimEnd('\0', ' ');
            }
        }
        catch (Exception ex)
        {
            // Return empty string if decoding fails
            Console.WriteLine($"Error decoding text frame: {ex.Message}");
            return string.Empty;
        }
    }

    private static bool IsValidFrameId(byte[] data, int offset, int length)
    {
        // A valid frame ID consists of uppercase letters A-Z and numbers 0-9
        if (offset + length > data.Length)
        {
            return false;
        }

        for (var i = 0; i < length; i++)
        {
            var b = data[offset + i];
            if (!((b >= 'A' && b <= 'Z') || (b >= '0' && b <= '9')))
            {
                return false;
            }
        }

        return true;
    }

    private static string ParseGenre(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Check for pattern like "(17)Rock" or "(17)" and extract both numeric code and text
        if (input.StartsWith("(") && input.Contains(")"))
        {
            var closeParenIndex = input.IndexOf(')');
            if (closeParenIndex > 1)
            {
                var numericPart = input.Substring(1, closeParenIndex - 1);

                // If there's text after the parentheses, use that
                if (closeParenIndex < input.Length - 1)
                {
                    return input.Substring(closeParenIndex + 1);
                }

                // Otherwise, try to convert the numeric code to a genre name
                if (int.TryParse(numericPart, out var genreCode))
                {
                    return GetGenreFromCode((byte)genreCode);
                }
            }
        }

        // No special format, return as is
        return input;
    }

    private static string GetGenreFromCode(byte code)
    {
        // Common ID3v1 genres
        string[] genres =
        {
            "Blues", "Classic Rock", "Country", "Dance", "Disco", "Funk", "Grunge", "Hip-Hop",
            "Jazz", "Metal", "New Age", "Oldies", "Other", "Pop", "R&B", "Rap", "Reggae",
            "Rock", "Techno", "Industrial", "Alternative", "Ska", "Death Metal", "Pranks",
            "Soundtrack", "Euro-Techno", "Ambient", "Trip-Hop", "Vocal", "Jazz+Funk", "Fusion",
            "Trance", "Classical", "Instrumental", "Acid", "House", "Game", "Sound Clip",
            "Gospel", "Noise", "AlternRock", "Bass", "Soul", "Punk", "Space", "Meditative",
            "Instrumental Pop", "Instrumental Rock", "Ethnic", "Gothic", "Darkwave",
            "Techno-Industrial", "Electronic", "Pop-Folk", "Eurodance", "Dream",
            "Southern Rock", "Comedy", "Cult", "Gangsta", "Top 40", "Christian Rap",
            "Pop/Funk", "Jungle", "Native American", "Cabaret", "New Wave", "Psychedelic",
            "Rave", "Showtunes", "Trailer", "Lo-Fi", "Tribal", "Acid Punk", "Acid Jazz",
            "Polka", "Retro", "Musical", "Rock & Roll", "Hard Rock"
        };

        if (code < genres.Length)
        {
            return genres[code];
        }

        return code.ToString(); // Return code as string if unknown
    }

    private void EnsureEmptyStringDefaults(Dictionary<MetaTagIdentifier, object> tags)
    {
        // Ensure we have empty strings for common text fields if they're missing
        var textTags = new[]
        {
            MetaTagIdentifier.Title,
            MetaTagIdentifier.Artist,
            MetaTagIdentifier.AlbumArtist,
            MetaTagIdentifier.Album,
            MetaTagIdentifier.RecordingYear,
            MetaTagIdentifier.TrackNumber,
            MetaTagIdentifier.DiscNumber,
            MetaTagIdentifier.Genre,
            MetaTagIdentifier.Composer,
            MetaTagIdentifier.Comment,
            MetaTagIdentifier.Copyright
        };

        foreach (var tag in textTags)
        {
            if (!tags.ContainsKey(tag))
            {
                tags[tag] = string.Empty;
            }
            else if (tags[tag] == null || string.IsNullOrWhiteSpace(tags[tag].ToString()))
            {
                // Replace null or whitespace-only values with empty string
                tags[tag] = string.Empty;
            }
            else if (tag == MetaTagIdentifier.TrackNumber || tag == MetaTagIdentifier.DiscNumber)
            {
                // Ensure numeric tags are strings
                tags[tag] = tags[tag].ToString();
            }
        }
    }
}
