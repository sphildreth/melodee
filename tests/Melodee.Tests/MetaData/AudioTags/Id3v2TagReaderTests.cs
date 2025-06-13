using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Melodee.Common.Metadata.AudioTags.Readers;
using Melodee.Common.Enums;
using System.Threading;
using Melodee.Common.Metadata.AudioTags;
using Melodee.Common.Utility;
using Melodee.Tests.Utility;
using System.Collections.Generic;

namespace Melodee.Tests.MetaData.AudioTags
{
    public class Id3v2TagReaderTests
    {
        private readonly string _testOutputPath;
        
        public Id3v2TagReaderTests()
        {
            // Create a test directory in the current directory
            _testOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Id3v2TestFiles");
            Directory.CreateDirectory(_testOutputPath);
        }
        
        [Fact]
        public async Task Read_Media_Files_In_Test_Folder()
        {
            var testFolder = Path.Combine(Directory.GetCurrentDirectory(), "/melodee_test/tests/good");
            if (!Directory.Exists(testFolder))
            {
                return;
            }
            var tags = await AudioTagManager.ReadAllTagsAsync(Path.Combine(testFolder, "test_3_2.mp3"), CancellationToken.None);
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
        
        [Fact]
        public async Task Should_Read_Tags_From_Generated_Id3v2_File()
        {
            // Arrange - Create a test file with specific metadata
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Test Title",
                Artist = "Test Artist",
                Album = "Test Album",
                Year = 2025,
                TrackNumber = 7,
                Genre = "Rock",
                Comment = "This is a test comment"
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Act - Read the tags
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Verify the tags match what we set
                Assert.Equal(AudioFormat.Mp3, tags.Format);
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                Assert.Contains(metadata.Comment, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Special_Characters_In_Tags()
        {
            // Arrange - Create a test file with special characters in metadata
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Special Characters: áéíóú ñ",
                Artist = "Testing & Symbols: !@#$%^&*()",
                Album = "Quotes \"Double\" and 'Single'",
                Year = 2025,
                TrackNumber = 1,
                Genre = "Test < > Genre",
                Comment = "Unicode: 你好, こんにちは, مرحبا"
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Act - Read the tags
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Verify special characters were preserved
                Assert.Contains("áéíóú", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Contains("Testing & Symbols", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Contains("Quotes", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Contains("Unicode:", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Empty_Tags()
        {
            // Arrange - Create a test file with empty metadata fields
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "",
                Artist = "",
                Album = "Test Album with Empty Fields",
                Year = 0,
                TrackNumber = 0,
                Genre = "",
                Comment = ""
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Act - Read the tags
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Verify empty fields are handled properly
                
                Assert.False(tags.IsValid());
                
                Assert.Equal(string.Empty, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(string.Empty, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal("Test Album with Empty Fields", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal("0", SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal("0", SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                Assert.Equal(string.Empty, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Genre]));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Very_Long_Tag_Values()
        {
            // Arrange - Create a test file with very long tag values
            string longString = new string('A', 10000); // 10,000 'A' characters
            
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Long Title " + longString,
                Artist = "Test Artist",
                Album = "Test Album",
                Year = 2025,
                TrackNumber = 1,
                Genre = "Test",
                Comment = "Long Comment " + longString
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Act - Read the tags
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Verify long strings are handled
                Assert.StartsWith("Long Title ", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.StartsWith("Long Comment ", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Fail_Reading_Non_Existent_File()
        {
            // Arrange - Non-existent file path
            string nonExistentFilePath = Path.Combine(_testOutputPath, "does-not-exist.mp3");
            
            // Act & Assert - Should throw an exception when trying to read a file that doesn't exist
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                AudioTagManager.ReadAllTagsAsync(nonExistentFilePath, CancellationToken.None));
        }
        
        [Fact]
        public async Task Should_Fail_Reading_Invalid_File_Format()
        {
            // Arrange - Create a text file with .mp3 extension
            string filePath = Path.Combine(_testOutputPath, "fake-mp3-file.mp3");
            await File.WriteAllTextAsync(filePath, "This is not a valid Mp3 file");
            
            try
            {
                // Act - Try to read the tags from an invalid file format
                var exception = await Record.ExceptionAsync(async () => 
                    await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None));
                
                // Assert - Should throw NotSupportedException for invalid format
                Assert.IsType<NotSupportedException>(exception);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Zero_Byte_Files()
        {
            // Arrange - Create an empty file with Mp3 extension
            string filePath = Path.Combine(_testOutputPath, "empty.mp3");
            File.Create(filePath).Close(); // Creates a zero-byte file
            
            try
            {
                // Act & Assert - Should throw NotSupportedException for invalid format
                var exception = await Record.ExceptionAsync(async () => 
                    await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None));
                
                Assert.IsType<NotSupportedException>(exception);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Cancellation_Token()
        {
            // Arrange - Create a test file
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Cancellation Test",
                Artist = "Test Artist"
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Create a cancellation token that's already canceled
                var cts = new CancellationTokenSource();
                cts.Cancel();
                
                // Act & Assert - Should throw when the token is already canceled
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
                    AudioTagManager.ReadAllTagsAsync(filePath, cts.Token));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        
        [Fact]
        public async Task Should_Read_Multiple_Generated_Files()
        {
            // Arrange - Generate multiple files with different metadata
            var files = new List<(string FilePath, BlankMusicFileGenerator.MusicMetadata Metadata)>();
            
            for (int i = 1; i <= 5; i++)
            {
                var metadata = new BlankMusicFileGenerator.MusicMetadata
                {
                    Title = $"Test Song {i}",
                    Artist = $"Test Artist {i}",
                    Album = "Test Album",
                    Year = 2020 + i,
                    TrackNumber = i,
                    Genre = "Test"
                };
                
                string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
                files.Add((filePath, metadata));
            }
            
            try
            {
                // Act & Assert - Read each file and verify its metadata
                foreach (var (filePath, metadata) in files)
                {
                    var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                    
                    Assert.Equal(AudioFormat.Mp3, tags.Format);
                    Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                    Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                    Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                }
            }
            finally
            {
                // Clean up
                foreach (var (filePath, _) in files)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }
        
        [Fact]
        public async Task Should_Handle_Corrupted_Tag_Data()
        {
            // Generate a valid file first
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Test Title",
                Artist = "Test Artist"
            };
            
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);
            
            try
            {
                // Corrupt the file by overwriting part of it
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    // Seek to position after the ID3 header (first 10 bytes) and corrupt some tag data
                    stream.Seek(15, SeekOrigin.Begin);
                    byte[] corruption = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                    stream.Write(corruption, 0, corruption.Length);
                }
                
                // Act - Try to read the corrupted file, but it might throw an exception
                var exception = await Record.ExceptionAsync(async () => 
                    await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None));
                
                // The test passes regardless of whether an exception is thrown, as we're testing 
                // that the system can handle corrupted tags without crashing unexpectedly
                if (exception == null)
                {
                    // If no exception, the format should still be correctly detected
                    var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                    Assert.Equal(AudioFormat.Mp3, tags.Format);
                }
                else
                {
                    // If exception, it should be a documented type (like FormatException, InvalidDataException, etc.)
                    Assert.IsNotType<NullReferenceException>(exception);
                    Assert.IsNotType<AccessViolationException>(exception);
                }
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
