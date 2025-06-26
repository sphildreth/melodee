using System.Text;
using Melodee.Common.Metadata.AudioTags;
using Xunit.Abstractions;

namespace Melodee.Tests.MetaData.AudioTags;

public class VideoFormatDetectorTests
{
    private readonly ITestOutputHelper _output;

    public VideoFormatDetectorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task IsVideoFileAsync_WithNonexistentFile_ReturnsFalse()
    {
        // Arrange
        var nonexistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.mp4");

        // Act
        var result = await VideoFormatDetector.IsVideoFileAsync(nonexistentFile, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsVideoFileAsync_WithEmptyFile_ReturnsFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + ".mp4");
        var emptyFile = tempFile + ".mp4";

        try
        {
            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(emptyFile, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (File.Exists(emptyFile))
            {
                File.Delete(emptyFile);
            }
        }
    }

    [Theory]
    [InlineData(".3gp")]
    [InlineData(".avi")]
    [InlineData(".flv")]
    [InlineData(".mkv")]
    [InlineData(".m4v")]
    [InlineData(".mov")]
    [InlineData(".mp4")]
    [InlineData(".webm")]
    [InlineData(".wmv")]
    public async Task IsVideoFileAsync_WithValidVideoFile_ReturnsTrue(string extension)
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + extension);
        var videoFile = tempFile + extension;

        try
        {
            // Create a minimal video file signature
            using (var fs = File.OpenWrite(videoFile))
            {
                switch (extension)
                {
                    case ".mp4":
                    case ".mov":
                    case ".m4v":
                        // Write MP4/MOV signature with video ftyp
                        fs.Write(new byte[] { 0, 0, 0, 0x20 }, 0, 4); // Box size
                        fs.Write(Encoding.ASCII.GetBytes("ftyp"), 0, 4);
                        fs.Write(Encoding.ASCII.GetBytes("mp42"), 0, 4); // Video brand
                        fs.Write(new byte[16], 0, 16); // Padding
                        fs.Write(Encoding.ASCII.GetBytes("vide"), 0, 4); // Video track identifier
                        break;
                    case ".avi":
                        // Write AVI signature
                        fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                        fs.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 4); // File size
                        fs.Write(Encoding.ASCII.GetBytes("AVI "), 0, 4);
                        break;
                    case ".mkv":
                    case ".webm":
                        // Write EBML/Matroska signature
                        fs.Write(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, 0, 4);
                        break;
                    case ".flv":
                        // Write FLV signature
                        fs.Write(Encoding.ASCII.GetBytes("FLV"), 0, 3);
                        fs.WriteByte(0x01); // Version
                        break;
                    case ".wmv":
                        // Write ASF header (same as WMA but with .wmv extension)
                        fs.Write(new byte[] { 0x30, 0x26, 0xB2, 0x75 }, 0, 4);
                        break;
                    default:
                        // For other formats, just write some data to make file non-empty
                        fs.Write(new byte[16], 0, 16);
                        break;
                }
            }

            var fileInfo = new FileInfo(videoFile);

            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(fileInfo.FullName, CancellationToken.None);

            // Assert
            Assert.True(result, $"File with extension {extension} should be detected as video");
        }
        finally
        {
            if (File.Exists(videoFile))
            {
                File.Delete(videoFile);
            }
        }
    }

    [Fact]
    public async Task IsVideoFileAsync_WithM4AAudioFile_ReturnsFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + ".m4a");
        var audioFile = tempFile + ".m4a";

        try
        {
            // Create M4A audio file signature
            using (var fs = File.OpenWrite(audioFile))
            {
                fs.Write(new byte[] { 0, 0, 0, 0x20 }, 0, 4); // Box size
                fs.Write(Encoding.ASCII.GetBytes("ftyp"), 0, 4);
                fs.Write(Encoding.ASCII.GetBytes("M4A "), 0, 4); // Audio brand
            }

            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(audioFile, CancellationToken.None);

            // Assert
            Assert.False(result, "M4A files should not be detected as video");
        }
        finally
        {
            if (File.Exists(audioFile))
            {
                File.Delete(audioFile);
            }
        }
    }

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".wav")]
    [InlineData(".flac")]
    [InlineData(".ogg")]
    [InlineData(".wma")]
    [InlineData(".m4a")]
    public async Task IsVideoFileAsync_WithAudioFiles_ReturnsFalse(string extension)
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + extension);
        var audioFile = tempFile + extension;

        try
        {
            // Create minimal audio file signatures
            using (var fs = File.OpenWrite(audioFile))
            {
                switch (extension)
                {
                    case ".mp3":
                        // Write MP3 sync word
                        fs.Write(new byte[] { 0xFF, 0xFB }, 0, 2);
                        break;
                    case ".wav":
                        // Write WAV signature
                        fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                        fs.Write(new byte[] { 0x24, 0, 0, 0 }, 0, 4);
                        fs.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
                        break;
                    case ".flac":
                        // Write FLAC signature
                        fs.Write(Encoding.ASCII.GetBytes("fLaC"), 0, 4);
                        break;
                    case ".ogg":
                        // Write OGG signature
                        fs.Write(Encoding.ASCII.GetBytes("OggS"), 0, 4);
                        break;
                    case ".wma":
                        // Write ASF header
                        fs.Write(new byte[] { 0x30, 0x26, 0xB2, 0x75 }, 0, 4);
                        break;
                    case ".m4a":
                        // Write M4A audio signature
                        fs.Write(new byte[] { 0, 0, 0, 0x20 }, 0, 4);
                        fs.Write(Encoding.ASCII.GetBytes("ftyp"), 0, 4);
                        fs.Write(Encoding.ASCII.GetBytes("M4A "), 0, 4);
                        break;
                }
            }

            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(audioFile, CancellationToken.None);

            // Assert
            Assert.False(result, $"Audio file with extension {extension} should not be detected as video");
        }
        finally
        {
            if (File.Exists(audioFile))
            {
                File.Delete(audioFile);
            }
        }
    }

    [Fact]
    public async Task IsVideoFileAsync_WithCorruptedFile_ReturnsFalse()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + ".mp4");
        var corruptedFile = tempFile + ".mp4";

        try
        {
            // Create a file that might cause read errors
            using (var fs = File.OpenWrite(corruptedFile))
            {
                // Write only partial header
                fs.Write(new byte[] { 0, 0 }, 0, 2);
            }

            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(corruptedFile, CancellationToken.None);

            // Assert
            Assert.False(result, "Corrupted files should not be detected as video");
        }
        finally
        {
            if (File.Exists(corruptedFile))
            {
                File.Delete(corruptedFile);
            }
        }
    }

    [Fact]
    public async Task IsVideoFileAsync_WithUnknownExtensionButVideoSignature_ReturnsTrue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, tempFile + ".unknown");
        var unknownFile = tempFile + ".unknown";

        try
        {
            // Create a file with video signature but unknown extension
            using (var fs = File.OpenWrite(unknownFile))
            {
                // Write AVI signature
                fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                fs.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 4);
                fs.Write(Encoding.ASCII.GetBytes("AVI "), 0, 4);
            }

            // Act
            var result = await VideoFormatDetector.IsVideoFileAsync(unknownFile, CancellationToken.None);

            // Assert
            Assert.True(result, "Files with video signatures should be detected regardless of extension");
        }
        finally
        {
            if (File.Exists(unknownFile))
            {
                File.Delete(unknownFile);
            }
        }
    }
}
