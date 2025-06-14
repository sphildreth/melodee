using System.Text;

namespace Melodee.Tests.Utility;

/// <summary>
///     Utility for creating empty music files with only metadata for testing purposes.
///     These files will have valid headers and tag structures but minimal or no actual audio data.
/// </summary>
public static class BlankMusicFileGenerator
{
    /// <summary>
    ///     ID3 version enumeration for test file generation
    /// </summary>
    public enum Id3Version
    {
        /// <summary>ID3v1.1 tags</summary>
        Id3v1_1,

        /// <summary>ID3v2.2 tags</summary>
        Id3v2_2,

        /// <summary>ID3v2.3 tags</summary>
        Id3v2_3,

        /// <summary>ID3v2.4 tags</summary>
        Id3v2_4,

        /// <summary>Multiple tag versions in the same file</summary>
        MultipleVersions
    }

    /// <summary>
    ///     Creates a minimal MP3 file with the specified ID3 version tags and no actual audio data.
    /// </summary>
    public static async Task<string> CreateMinimalMp3FileWithVersionAsync(
        string outputPath,
        Id3Version version,
        MusicMetadata? metadata = null)
    {
        metadata ??= new MusicMetadata();
        var versionString = version.ToString().Replace("_", ".");
        var filePath = Path.Combine(outputPath, $"test_{versionString}_{Guid.NewGuid():N}.mp3");

        Directory.CreateDirectory(outputPath);

        // Create the base MP3 file with just the frame header
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            // Add minimal MP3 frame header without audio data (we'll add tags before/after this)
            var frameHeader = new byte[]
            {
                0xFF, 0xFB, 0x90, 0x44, // MPEG-1 Layer 3, 128kbps, 44.1kHz
                0x00, 0x00, 0x00, 0x00 // Minimal frame data
            };
            fileStream.Write(frameHeader, 0, frameHeader.Length);
        }

        // Now add the ID3 tags according to the requested version
        if (version == Id3Version.Id3v1_1 || version == Id3Version.MultipleVersions)
        {
            await AddId3v1TagsAsync(filePath, metadata);
        }

        if (version != Id3Version.Id3v1_1) // Any ID3v2 version
        {
            await AddId3v2TagsAsync(filePath, version, metadata);
        }

        return filePath;
    }

    /// <summary>
    ///     Creates a minimal MP3 file with ID3v2 tags but no actual audio data.
    ///     Defaults to ID3v2.3 for compatibility.
    /// </summary>
    public static async Task<string> CreateMinimalMp3FileAsync(string outputPath, MusicMetadata? metadata = null)
    {
        // Debug information to trace the issue
        if (metadata != null)
        {
            Console.WriteLine($"CreateMinimalMp3FileAsync - Input Title: {metadata.Title}");
        }
        else
        {
            Console.WriteLine("CreateMinimalMp3FileAsync - Input metadata is null");
            metadata = new MusicMetadata();
            Console.WriteLine($"CreateMinimalMp3FileAsync - Default Title: {metadata.Title}");
        }

        // Make sure we're not using a hardcoded title in test_file1
        return await CreateMinimalMp3FileWithVersionAsync(outputPath, Id3Version.Id3v2_3, metadata);
    }

    private static void WriteFlacVorbisComment(BinaryWriter writer, string name, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var comment = $"{name}={value}";
        var commentBytes = Encoding.UTF8.GetBytes(comment);

        // Write comment length as big endian (FLAC standard)
        writer.Write(ReverseBytes(BitConverter.GetBytes((uint)commentBytes.Length)));

        // Write the comment data
        writer.Write(commentBytes);

        Console.WriteLine($"Writing FLAC comment: {name}={value}, length: {commentBytes.Length}");
    }

    /// <summary>
    ///     Creates a minimal OGG Vorbis file with metadata but no actual audio data.
    /// </summary>
    public static async Task<string> CreateMinimalVorbisFileAsync(string outputPath, MusicMetadata? metadata = null)
    {
        metadata ??= new MusicMetadata();
        var filePath = Path.Combine(outputPath, $"test_minimal_{Guid.NewGuid():N}.ogg");

        Directory.CreateDirectory(outputPath);

        using (var writer = new BinaryWriter(File.Create(filePath)))
        {
            var serialNumber = new Random().Next();

            // Write identification header page
            // OGG page header
            writer.Write(new byte[] { 0x4F, 0x67, 0x67, 0x53 }); // "OggS" marker
            writer.Write((byte)0x00); // Version
            writer.Write((byte)0x02); // Header type (start of logical bitstream)

            // Granule position (8 bytes)
            writer.Write(new byte[8]);

            // Stream serial number (random)
            writer.Write(BitConverter.GetBytes(serialNumber));

            // Page sequence number
            writer.Write(BitConverter.GetBytes(0));

            // CRC checksum (will be calculated later, use placeholder)
            var crcPosition1 = writer.BaseStream.Position;
            writer.Write(BitConverter.GetBytes(0));

            // Number of page segments
            writer.Write((byte)1);

            // Segment table
            writer.Write((byte)30); // First segment size

            var headerStartPosition = writer.BaseStream.Position;

            // Vorbis identification header
            writer.Write((byte)0x01);
            writer.Write(new byte[] { 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 }); // "vorbis"

            // Vorbis version (0)
            writer.Write(BitConverter.GetBytes(0));

            // Audio channels (2)
            writer.Write((byte)2);

            // Sample rate (44100)
            writer.Write(BitConverter.GetBytes(44100));

            // Bitrate maximum, nominal, minimum
            writer.Write(BitConverter.GetBytes(0)); // Maximum
            writer.Write(BitConverter.GetBytes(128000)); // Nominal 
            writer.Write(BitConverter.GetBytes(0)); // Minimum

            // blocksize_0 (2^8) and blocksize_1 (2^10)
            writer.Write((byte)0x80);

            // Framing flag
            writer.Write((byte)0x01);

            // Now write the comment header in a second page

            // Prepare Vorbis comments
            using (var commentStream = new MemoryStream())
            using (var commentWriter = new BinaryWriter(commentStream))
            {
                // Comment header type
                commentWriter.Write((byte)0x03);
                commentWriter.Write(new byte[] { 0x76, 0x6F, 0x72, 0x62, 0x69, 0x73 }); // "vorbis"

                // Vendor string length and content
                var vendor = "Melodee Test";
                commentWriter.Write(BitConverter.GetBytes(vendor.Length));
                commentWriter.Write(Encoding.UTF8.GetBytes(vendor));

                // Count of user comments
                var commentCount = 0;
                if (!string.IsNullOrEmpty(metadata.Title))
                {
                    commentCount++;
                }

                if (!string.IsNullOrEmpty(metadata.Artist))
                {
                    commentCount++;
                }

                if (!string.IsNullOrEmpty(metadata.Album))
                {
                    commentCount++;
                }

                if (!string.IsNullOrEmpty(metadata.Genre))
                {
                    commentCount++;
                }

                if (!string.IsNullOrEmpty(metadata.Comment))
                {
                    commentCount++;
                }

                if (metadata.RecordingYear > 0)
                {
                    commentCount++;
                }

                if (metadata.TrackNumber > 0)
                {
                    commentCount++;
                }

                commentWriter.Write(BitConverter.GetBytes(commentCount));

                // Write each comment
                WriteVorbisComment(commentWriter, "TITLE", metadata.Title);
                WriteVorbisComment(commentWriter, "ARTIST", metadata.Artist);
                WriteVorbisComment(commentWriter, "ALBUM", metadata.Album);
                WriteVorbisComment(commentWriter, "GENRE", metadata.Genre);
                WriteVorbisComment(commentWriter, "DESCRIPTION", metadata.Comment);
                if (metadata.RecordingYear > 0)
                {
                    WriteVorbisComment(commentWriter, "DATE", metadata.RecordingYear.ToString());
                }

                if (metadata.TrackNumber > 0)
                {
                    WriteVorbisComment(commentWriter, "TRACKNUMBER", metadata.TrackNumber.ToString());
                }

                // Framing bit
                commentWriter.Write((byte)0x01);

                // Get the comment data
                var commentData = commentStream.ToArray();

                // Write OGG page for comment header
                writer.Write(new byte[] { 0x4F, 0x67, 0x67, 0x53 }); // "OggS" marker
                writer.Write((byte)0x00); // Version
                writer.Write((byte)0x00); // Continuation page

                // Granule position (8 bytes)
                writer.Write(new byte[8]);

                // Stream serial number (same as first page)
                writer.Write(BitConverter.GetBytes(serialNumber));

                // Page sequence number
                writer.Write(BitConverter.GetBytes(1));

                // CRC checksum (will be calculated later, use placeholder)
                var crcPosition2 = writer.BaseStream.Position;
                writer.Write(BitConverter.GetBytes(0));

                // Number of page segments and segment table
                // Split comment data into segments if needed
                var segmentTable = new List<byte>();
                var remainingBytes = commentData.Length;
                while (remainingBytes > 255)
                {
                    segmentTable.Add(255);
                    remainingBytes -= 255;
                }

                segmentTable.Add((byte)remainingBytes);

                writer.Write((byte)segmentTable.Count);
                foreach (var segment in segmentTable)
                {
                    writer.Write(segment);
                }

                var commentHeaderStartPosition = writer.BaseStream.Position;

                // Write the actual comment data
                writer.Write(commentData);

                // Add a third page with EOS flag to properly finish the stream
                writer.Write(new byte[] { 0x4F, 0x67, 0x67, 0x53 }); // "OggS" marker
                writer.Write((byte)0x00); // Version
                writer.Write((byte)0x04); // End of stream

                // Granule position (8 bytes)
                writer.Write(new byte[8]);

                // Stream serial number (same as first page)
                writer.Write(BitConverter.GetBytes(serialNumber));

                // Page sequence number
                writer.Write(BitConverter.GetBytes(2));

                // CRC checksum (placeholder)
                writer.Write(BitConverter.GetBytes(0));

                // Empty segment table
                writer.Write((byte)0); // 0 segments
            }
        }

        return filePath;
    }

    private static void WriteVorbisComment(BinaryWriter writer, string name, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var comment = $"{name}={value}";
        var commentBytes = Encoding.UTF8.GetBytes(comment);

        // Write comment length as little endian (standard for Vorbis comments)
        writer.Write(BitConverter.GetBytes((uint)commentBytes.Length));

        // Write the comment data
        writer.Write(commentBytes);

        // Debug
        Console.WriteLine($"Writing Vorbis comment: {name}={value}, length: {commentBytes.Length}");
    }

    /// <summary>
    ///     Creates a batch of test files in all supported formats
    /// </summary>
    public static async Task<List<string>> CreateTestFileBatchAsync(string outputPath, MusicMetadata? metadata = null)
    {
        var files = new List<string>();

        // Ensure we're using the provided metadata for all file types
        metadata ??= new MusicMetadata();

        files.Add(await CreateMinimalMp3FileAsync(outputPath, metadata));
        files.Add(await CreateMinimalVorbisFileAsync(outputPath, metadata));

        return files;
    }

    /// <summary>
    ///     Standard metadata that can be included in test files
    /// </summary>
    public class MusicMetadata
    {
        public string Title { get; set; } = "Test Title";
        public string Artist { get; set; } = "Test Artist";
        public string Album { get; set; } = "Test Album";
        public int RecordingYear { get; set; } = 2025;
        public int TrackNumber { get; set; } = 1;
        public string Genre { get; set; } = "Test";
        public string Comment { get; set; } = "This is a test file with no actual audio data";
        public byte[]? AlbumArt { get; set; } = null;
    }

    #region Helper Methods

    /// <summary>
    ///     Adds ID3v1.1 tags to an existing MP3 file
    /// </summary>
    private static async Task AddId3v1TagsAsync(string filePath, MusicMetadata metadata)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
        {
            // Seek to the end of the file to append ID3v1 tag
            fileStream.Seek(0, SeekOrigin.End);

            using (var writer = new BinaryWriter(fileStream, Encoding.UTF8, true))
            {
                // TAG marker for ID3v1
                writer.Write(Encoding.ASCII.GetBytes("TAG"));

                // Title (30 bytes, padded with nulls)
                WriteFixedLengthString(writer, metadata.Title, 30);

                // Artist (30 bytes, padded with nulls)
                WriteFixedLengthString(writer, metadata.Artist, 30);

                // Album (30 bytes, padded with nulls)
                WriteFixedLengthString(writer, metadata.Album, 30);

                // Year (4 bytes, padded with spaces)
                WriteFixedLengthString(writer, metadata.RecordingYear.ToString(), 4);

                // Comment (28 bytes for ID3v1.1, padded with nulls)
                WriteFixedLengthString(writer, metadata.Comment, 28);

                // Zero byte separator (ID3v1.1 extension)
                writer.Write((byte)0);

                // Track number (ID3v1.1 extension)
                writer.Write((byte)metadata.TrackNumber);

                // Genre (1 byte)
                writer.Write((byte)255); // 255 = undefined genre
            }
        }
    }

    /// <summary>
    ///     Adds ID3v2 tags to an existing MP3 file
    /// </summary>
    private static async Task AddId3v2TagsAsync(string filePath, Id3Version version, MusicMetadata metadata)
    {
        var fileContent = await File.ReadAllBytesAsync(filePath);

        // We'll use a memory stream for building the tag data
        using (var ms = new MemoryStream())
        {
            // Create a writer for the memory stream
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) // leaveOpen = true
            {
                // Reserve space for the header (we'll write it at the end)
                writer.Write(new byte[10]);

                int majorVersion;

                switch (version)
                {
                    case Id3Version.Id3v2_2:
                        majorVersion = 2;
                        WriteId3v2_2Frames(writer, metadata);
                        break;
                    case Id3Version.Id3v2_4:
                        majorVersion = 4;
                        WriteId3v2_4Frames(writer, metadata);
                        break;
                    case Id3Version.MultipleVersions:
                        // For MultipleVersions, default to v2.3 as it's most common
                        majorVersion = 3;
                        WriteId3v2_3Frames(writer, metadata);
                        break;
                    case Id3Version.Id3v2_3:
                    default:
                        majorVersion = 3;
                        WriteId3v2_3Frames(writer, metadata);
                        break;
                }

                // Get the entire tag size (excluding the header)
                var tagSize = (int)ms.Length - 10;

                // Go back to the beginning to write the header with the correct size
                ms.Seek(0, SeekOrigin.Begin);
                writer.Write(Encoding.ASCII.GetBytes("ID3"));
                writer.Write((byte)majorVersion); // Version
                writer.Write((byte)0); // Revision
                writer.Write((byte)0); // Flags

                // Write size as a syncsafe integer (7 bits per byte)
                writer.Write((byte)((tagSize >> 21) & 0x7F));
                writer.Write((byte)((tagSize >> 14) & 0x7F));
                writer.Write((byte)((tagSize >> 7) & 0x7F));
                writer.Write((byte)(tagSize & 0x7F));
            }

            // Now write the complete file with ID3v2 tag at the beginning
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                // Reset position to beginning before copying
                ms.Position = 0;

                // Write the ID3v2 tag
                ms.CopyTo(fileStream);

                // Write the original file content
                fileStream.Write(fileContent, 0, fileContent.Length);
            }
        }
    }

    /// <summary>
    ///     Write ID3v2.2 frames
    /// </summary>
    private static void WriteId3v2_2Frames(BinaryWriter writer, MusicMetadata metadata)
    {
        // ID3v2.2 uses 3-character frame IDs
        WriteId3v2_2TextFrame(writer, "TT2", metadata.Title); // Title
        WriteId3v2_2TextFrame(writer, "TP1", metadata.Artist); // Artist
        WriteId3v2_2TextFrame(writer, "TAL", metadata.Album); // Album 
        WriteId3v2_2TextFrame(writer, "TRK", metadata.TrackNumber.ToString()); // Track number
        WriteId3v2_2TextFrame(writer, "TYE", metadata.RecordingYear.ToString()); // Year
        WriteId3v2_2TextFrame(writer, "TCO", metadata.Genre); // Genre
        WriteId3v2_2CommentFrame(writer, metadata.Comment); // Comment
    }

    /// <summary>
    ///     Write ID3v2.3 frames
    /// </summary>
    private static void WriteId3v2_3Frames(BinaryWriter writer, MusicMetadata metadata)
    {
        WriteId3v2TextFrame(writer, "TIT2", metadata.Title);
        WriteId3v2TextFrame(writer, "TPE1", metadata.Artist);
        WriteId3v2TextFrame(writer, "TALB", metadata.Album);
        WriteId3v2TextFrame(writer, "TRCK", metadata.TrackNumber.ToString());
        WriteId3v2TextFrame(writer, "TYER", metadata.RecordingYear.ToString());
        WriteId3v2TextFrame(writer, "TCON", metadata.Genre);
        WriteId3v2CommentFrame(writer, metadata.Comment);
    }

    /// <summary>
    ///     Write ID3v2.4 frames
    /// </summary>
    private static void WriteId3v2_4Frames(BinaryWriter writer, MusicMetadata metadata)
    {
        WriteId3v2TextFrame(writer, "TIT2", metadata.Title);
        WriteId3v2TextFrame(writer, "TPE1", metadata.Artist);
        WriteId3v2TextFrame(writer, "TALB", metadata.Album);
        WriteId3v2TextFrame(writer, "TRCK", metadata.TrackNumber.ToString());
        WriteId3v2TextFrame(writer, "TDRC", metadata.RecordingYear.ToString()); // v2.4 uses TDRC instead of TYER
        WriteId3v2TextFrame(writer, "TCON", metadata.Genre);
        WriteId3v2CommentFrame(writer, metadata.Comment);

        // Add album art if provided
        if (metadata.AlbumArt != null && metadata.AlbumArt.Length > 0)
        {
            WriteApicFrame(writer, metadata.AlbumArt);
        }
    }

    /// <summary>
    ///     Write a fixed-length string, padding with nulls or truncating as needed
    /// </summary>
    private static void WriteFixedLengthString(BinaryWriter writer, string text, int length)
    {
        var bytes = Encoding.ASCII.GetBytes((text ?? "").PadRight(length).Substring(0, length));
        writer.Write(bytes);
    }

    /// <summary>
    ///     Write an ID3v2.2 text frame (3-character frame ID, 3-byte size)
    /// </summary>
    private static void WriteId3v2_2TextFrame(BinaryWriter writer, string frameId, string text)
    {
        // Convert text to ISO-8859-1 (ID3v2.2 typically uses this)
        var textBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);

        // Frame ID (3 characters for v2.2)
        writer.Write(Encoding.ASCII.GetBytes(frameId));

        // Frame size (3 bytes for v2.2, excluding frame header)
        var size = textBytes.Length + 1; // +1 for encoding byte
        writer.Write((byte)((size >> 16) & 0xFF));
        writer.Write((byte)((size >> 8) & 0xFF));
        writer.Write((byte)(size & 0xFF));

        // Text encoding (ISO-8859-1 = 0)
        writer.Write((byte)0);

        // The actual text
        writer.Write(textBytes);
    }

    /// <summary>
    ///     Write an ID3v2.2 comment frame
    /// </summary>
    private static void WriteId3v2_2CommentFrame(BinaryWriter writer, string comment)
    {
        // Convert comment to ISO-8859-1
        var commentBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(comment);

        // Frame ID (COM for v2.2)
        writer.Write(Encoding.ASCII.GetBytes("COM"));

        // Frame size (excluding header)
        var size = commentBytes.Length + 5; // +1 for encoding, +3 for language, +1 for empty description
        writer.Write((byte)((size >> 16) & 0xFF));
        writer.Write((byte)((size >> 8) & 0xFF));
        writer.Write((byte)(size & 0xFF));

        // Text encoding (ISO-8859-1 = 0)
        writer.Write((byte)0);

        // Language (eng)
        writer.Write(Encoding.ASCII.GetBytes("eng"));

        // Short content description (empty string, terminated by null)
        writer.Write((byte)0);

        // The actual comment text
        writer.Write(commentBytes);
    }

    private static void WriteId3v2TextFrame(BinaryWriter writer, string frameId, string text)
    {
        // Convert text to UTF-8
        var textBytes = Encoding.UTF8.GetBytes(text);

        // Frame ID (4 characters)
        writer.Write(Encoding.ASCII.GetBytes(frameId));

        // Frame size (excluding header, not syncsafe in ID3v2.3)
        var size = textBytes.Length + 1; // +1 for encoding byte
        writer.Write((byte)((size >> 24) & 0xFF));
        writer.Write((byte)((size >> 16) & 0xFF));
        writer.Write((byte)((size >> 8) & 0xFF));
        writer.Write((byte)(size & 0xFF));

        // Frame flags (both zero)
        writer.Write((byte)0);
        writer.Write((byte)0);

        // Text encoding (UTF-8 = 3)
        writer.Write((byte)3);

        // The actual text
        writer.Write(textBytes);
    }

    private static void WriteId3v2CommentFrame(BinaryWriter writer, string comment)
    {
        // Convert comment to UTF-8
        var commentBytes = Encoding.UTF8.GetBytes(comment);

        // Frame ID (COMM)
        writer.Write(Encoding.ASCII.GetBytes("COMM"));

        // Frame size (excluding header, not syncsafe in ID3v2.3)
        var size = commentBytes.Length + 5; // +1 for encoding, +3 for language, +1 for empty description
        writer.Write((byte)((size >> 24) & 0xFF));
        writer.Write((byte)((size >> 16) & 0xFF));
        writer.Write((byte)((size >> 8) & 0xFF));
        writer.Write((byte)(size & 0xFF));

        // Frame flags (both zero)
        writer.Write((byte)0);
        writer.Write((byte)0);

        // Text encoding (UTF-8 = 3)
        writer.Write((byte)3);

        // Language (eng)
        writer.Write(Encoding.ASCII.GetBytes("eng"));

        // Short content description (empty string, terminated by null)
        writer.Write((byte)0);

        // The actual comment text
        writer.Write(commentBytes);
    }

    private static void WriteTextFrame(BinaryWriter writer, string frameId, string text)
    {
        // Convert text to UTF-8
        var textBytes = Encoding.UTF8.GetBytes(text);

        // Frame ID (4 characters)
        writer.Write(Encoding.ASCII.GetBytes(frameId));

        // Frame size (excluding header, no synchsafe)
        writer.Write(ReverseBytes(BitConverter.GetBytes((uint)(textBytes.Length + 1))));

        // Frame flags (both zero)
        writer.Write((ushort)0);

        // Text encoding (UTF-8)
        writer.Write((byte)0x03);

        // The actual text
        writer.Write(textBytes);
    }

    private static void WriteCommentFrame(BinaryWriter writer, string comment)
    {
        // Convert comment to UTF-8
        var textBytes = Encoding.UTF8.GetBytes(comment);

        // Frame ID (COMM)
        writer.Write(Encoding.ASCII.GetBytes("COMM"));

        // Frame size (excluding header)
        writer.Write(ReverseBytes(BitConverter.GetBytes((uint)(textBytes.Length + 5))));

        // Frame flags (both zero)
        writer.Write((ushort)0);

        // Text encoding (UTF-8)
        writer.Write((byte)0x03);

        // Language (eng)
        writer.Write(Encoding.ASCII.GetBytes("eng"));

        // Short content description (empty string)
        writer.Write((byte)0x00);

        // The actual comment text
        writer.Write(textBytes);
    }

    /// <summary>
    ///     Write an APIC (album art) frame for ID3v2.3/v2.4
    /// </summary>
    private static void WriteApicFrame(BinaryWriter writer, byte[] imageData)
    {
        // Frame ID (APIC)
        writer.Write(Encoding.ASCII.GetBytes("APIC"));

        // Calculate total size of frame content
        var mimeType = "image/png"; // Assuming PNG for simplicity
        var mimeTypeBytes = Encoding.ASCII.GetBytes(mimeType);

        // Total size = encoding byte (1) + mime type + null terminator (1) + 
        // picture type (1) + description + null terminator (1) + image data
        var size = 1 + mimeTypeBytes.Length + 1 + 1 + 1 + imageData.Length;

        // Frame size (excluding header, not syncsafe in ID3v2.3, but should be in ID3v2.4)
        // For simplicity we'll use the same format for both
        writer.Write((byte)((size >> 24) & 0xFF));
        writer.Write((byte)((size >> 16) & 0xFF));
        writer.Write((byte)((size >> 8) & 0xFF));
        writer.Write((byte)(size & 0xFF));

        // Frame flags (both zero)
        writer.Write((byte)0);
        writer.Write((byte)0);

        // Text encoding (UTF-8 = 3)
        writer.Write((byte)3);

        // MIME type string (e.g., "image/png") followed by null terminator
        writer.Write(mimeTypeBytes);
        writer.Write((byte)0);

        // Picture type (3 = Cover/front)
        writer.Write((byte)3);

        // Description (empty string, UTF-8 encoded, null-terminated)
        writer.Write((byte)0);

        // The image data
        writer.Write(imageData);
    }

    private static byte[] ReverseBytes(byte[] bytes)
    {
        // Reverse byte order for big-endian values
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    #endregion
}
