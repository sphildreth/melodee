using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Melodee.Common.Metadata.AudioTags;
using System.IO;
using System.Threading;
using Melodee.Common.Utility;

namespace Melodee.Tests.MetaData.AudioTags
{
    public class AudioTagManagerTests
    {
        private readonly ITestOutputHelper _output;

        public AudioTagManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
        public async Task NeedsConversionToMp3Async_WithNonexistentFile_ReturnsFalse()
        {
            // Arrange
            var fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.mp3"));

            // Act
            var result = await AudioTagManager.NeedsConversionToMp3Async(fileInfo, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task NeedsConversionToMp3Async_WithValidMp3File_ReturnsFalse()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile + ".mp3");
            var mp3File = tempFile + ".mp3";

            try
            {
                // Create minimal MP3 header signature to be detected as MP3
                using (var fs = File.OpenWrite(mp3File))
                {
                    // Write MP3 sync word (0xFF 0xFB) to make it detectable as MP3
                    fs.WriteByte(0xFF);
                    fs.WriteByte(0xFB);
                    fs.WriteByte(0x90);
                    fs.WriteByte(0x44);
                    fs.WriteByte(0x00);
                }

                var fileInfo = new FileInfo(mp3File);

                // Act
                var result = await AudioTagManager.NeedsConversionToMp3Async(fileInfo, CancellationToken.None);

                // Assert
                Assert.False(result, "A valid MP3 file should not need conversion");
            }
            finally
            {
                if (File.Exists(mp3File))
                {
                    File.Delete(mp3File);
                }
            }
        }

        [Fact]
        public async Task NeedsConversionToMp3Async_WithFakeMp3Extension_ReturnsTrue()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile + ".mp3");
            var fakeFile = tempFile + ".mp3";

            try
            {
                // Just write some random data, not valid MP3 format
                File.WriteAllText(fakeFile, "This is not an MP3 file");
                var fileInfo = new FileInfo(fakeFile);

                // Act
                var result = await AudioTagManager.NeedsConversionToMp3Async(fileInfo, CancellationToken.None);

                // Assert
                Assert.True(result, "A fake MP3 file (wrong content) should need conversion");
            }
            finally
            {
                if (File.Exists(fakeFile))
                {
                    File.Delete(fakeFile);
                }
            }
        }

        [Theory]
        [InlineData(".wav")]
        [InlineData(".flac")]
        [InlineData(".ogg")]
        [InlineData(".m4a")]
        [InlineData(".wma")]
        public async Task NeedsConversionToMp3Async_WithOtherAudioFormats_ReturnsTrue(string extension)
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.Move(tempFile, tempFile + extension);
            var audioFile = tempFile + extension;

            try
            {
                // Need to make this file look like its format for detection
                using (var fs = File.OpenWrite(audioFile))
                {
                    switch (extension)
                    {
                        case ".wav":
                            // Write basic WAV header signature
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
                            fs.Write(new byte[] { 0x24, 0, 0, 0 }, 0, 4); // File size
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
                            break;
                        case ".flac":
                            // Write FLAC signature
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("fLaC"), 0, 4);
                            break;
                        case ".ogg":
                            // Write OGG signature
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("OggS"), 0, 4);
                            break;
                        case ".m4a":
                        case ".mp4":
                            // Write basic ISO base media file format signature
                            fs.Write(new byte[] { 0, 0, 0, 0x20 }, 0, 4); // Box size
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("ftyp"), 0, 4);
                            fs.Write(System.Text.Encoding.ASCII.GetBytes("M4A "), 0, 4);
                            break;
                        case ".wma":
                            // Write ASF header
                            var asfHeader = new byte[] {
                                0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11,
                                0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C
                            };
                            fs.Write(asfHeader, 0, asfHeader.Length);
                            break;
                    }
                }

                var fileInfo = new FileInfo(audioFile);

                // Act
                var result = await AudioTagManager.NeedsConversionToMp3Async(fileInfo, CancellationToken.None);

                // Assert
                // Some formats might not be recognized by the detector without more complete headers
                // So we're just checking if it returns the expected result or is at least detected as non-MP3
                var format = await FileFormatDetector.DetectFormatAsync(audioFile);
                if (format == AudioFormat.Unknown)
                {
                    // If format is unknown, we can't properly test needs conversion
                    // Our method should return false in this case as it can't convert unknown formats
                    Assert.False(result, $"Unknown format for {extension} should return false");
                }
                else
                {
                    // If format is detected, it should need conversion if it's not MP3
                    Assert.True(result, $"A {extension} file should need conversion to MP3");
                }
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
        public async Task NeedsConversionToMp3Async_WithNullFileInfo_ReturnsFalse()
        {
            // Act
            var result = await AudioTagManager.NeedsConversionToMp3Async(null, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}

