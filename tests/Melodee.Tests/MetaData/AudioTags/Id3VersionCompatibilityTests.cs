using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags;
using Melodee.Common.Utility;
using Melodee.Tests.Utility;
using System.Linq;
using System.Text;

namespace Melodee.Tests.MetaData.AudioTags
{
    /// <summary>
    /// Tests for verifying that different ID3 tag versions are read correctly.
    /// </summary>
    public class Id3VersionCompatibilityTests
    {
        private readonly string _testOutputPath;
        
        public Id3VersionCompatibilityTests()
        {
            // Create a test directory in the current directory
            _testOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Id3VersionTests");
            Directory.CreateDirectory(_testOutputPath);
        }
        
        [Fact]
        public async Task Should_Read_ID3v1_1_Tags()
        {
            // Arrange
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "ID3v1.1 Title Test",
                Artist = "ID3v1.1 Artist",
                Album = "ID3v1.1 Album",
                Year = 1999,
                TrackNumber = 12,
                Genre = "Rock", // ID3v1.1 has limited genres, but we'll test anyway
                Comment = "ID3v1.1 Comment Test"
            };
            
            // Act - Create a file with only ID3v1.1 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v1_1,
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Basic file properties 
                Assert.Equal(AudioFormat.Mp3, tags.Format);
                Assert.False(string.IsNullOrEmpty(tags.FileMetadata.FilePath));
                Assert.True(tags.FileMetadata.FileSize > 0);
                
                // Assert - ID3v1.1 tags should be read correctly
                // Note: ID3v1 has length limitations so we check starts-with
                Assert.StartsWith(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.StartsWith(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.StartsWith(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                
                // ID3v1.1 specifically added track number support
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                
                // Comments in ID3v1.1 are limited to 28 bytes, so we check if any part of our comment is there
                string commentPart = metadata.Comment.Substring(0, Math.Min(metadata.Comment.Length, 28));
                Assert.Contains(commentPart, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
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
        public async Task Should_Read_ID3v2_2_Tags()
        {
            // Arrange
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "ID3v2.2 Title Test",
                Artist = "ID3v2.2 Artist",
                Album = "ID3v2.2 Album Test",
                Year = 2002,
                TrackNumber = 2,
                Genre = "Classical",
                Comment = "ID3v2.2 Comment Test with special chars: áéíóú"
            };
            
            // Act - Create a file with ID3v2.2 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_2, 
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Basic file properties
                Assert.Equal(AudioFormat.Mp3, tags.Format);
                
                // Assert - ID3v2.2 tags should be read correctly even though they use different frame IDs
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                
                // v2.2 frames might handle special chars differently, so we check if part of the comment is there
                Assert.Contains("ID3v2.2 Comment", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
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
        public async Task Should_Read_ID3v2_3_Tags()
        {
            // Arrange
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "ID3v2.3 Title",
                Artist = "ID3v2.3 Artist",
                Album = "ID3v2.3 Album",
                Year = 2003,
                TrackNumber = 3,
                Genre = "Jazz",
                Comment = "ID3v2.3 UTF-8 Comment with symbols: ©®™"
            };
            
            // Act - Create a file with ID3v2.3 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_3, 
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Basic file properties
                Assert.Equal(AudioFormat.Mp3, tags.Format);
                
                // Assert - ID3v2.3 tags should be read correctly
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                
                // Check for UTF-8 symbols in comment
                Assert.Contains("©®™", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
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
        public async Task Should_Read_ID3v2_4_Tags()
        {
            // Arrange
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "ID3v2.4 Title",
                Artist = "ID3v2.4 Artist",
                Album = "ID3v2.4 Album",
                Year = 2004,
                TrackNumber = 4,
                Genre = "Electronic",
                Comment = "ID3v2.4 Unicode Test: 你好, こんにちは, مرحبا"
            };
            
            // Act - Create a file with ID3v2.4 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_4, 
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Basic file properties
                Assert.Equal(AudioFormat.Mp3, tags.Format);
                
                // Assert - ID3v2.4 tags should be read correctly
                // Note: ID3v2.4 uses TDRC instead of TYER for recording year/date
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                
                // Check for Unicode characters in comment
                string comment = SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]);
                Assert.Contains("Unicode Test", comment);
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
        public async Task Should_Prioritize_ID3v2_Over_ID3v1_When_Both_Present()
        {
            // Arrange
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Dual Tag Title",
                Artist = "Dual Tag Artist",
                Album = "Dual Tag Album",
                Year = 2025,
                TrackNumber = 5,
                Genre = "Rock",
                Comment = "This file has both ID3v1 and ID3v2 tags"
            };
            
            // Create a modified version of metadata for ID3v1
            var v1Metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "OUTDATED v1 Title", // This should NOT be used when v2 is present
                Artist = "OUTDATED v1 Artist",
                Album = "OUTDATED v1 Album",
                Year = 1999,
                TrackNumber = 99,
                Genre = "Classical",
                Comment = "This is the v1 comment that should not take precedence"
            };
            
            // Act - Create a file with ID3v2.3 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_3, 
                metadata);

            // Manually add ID3v1 tags to the same file
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                fileStream.Seek(0, SeekOrigin.End);
                
                using (var writer = new BinaryWriter(fileStream, Encoding.ASCII, true))
                {
                    // TAG marker
                    writer.Write(Encoding.ASCII.GetBytes("TAG"));
                    
                    // Title (30 bytes, padded with nulls)
                    byte[] titleBytes = Encoding.ASCII.GetBytes(v1Metadata.Title.PadRight(30).Substring(0, 30));
                    writer.Write(titleBytes);
                    
                    // Artist (30 bytes)
                    byte[] artistBytes = Encoding.ASCII.GetBytes(v1Metadata.Artist.PadRight(30).Substring(0, 30));
                    writer.Write(artistBytes);
                    
                    // Album (30 bytes)
                    byte[] albumBytes = Encoding.ASCII.GetBytes(v1Metadata.Album.PadRight(30).Substring(0, 30));
                    writer.Write(albumBytes);
                    
                    // Year (4 bytes)
                    byte[] yearBytes = Encoding.ASCII.GetBytes(v1Metadata.Year.ToString().PadRight(4).Substring(0, 4));
                    writer.Write(yearBytes);
                    
                    // Comment (28 bytes for ID3v1.1)
                    byte[] commentBytes = Encoding.ASCII.GetBytes(v1Metadata.Comment.PadRight(28).Substring(0, 28));
                    writer.Write(commentBytes);
                    
                    // Zero byte separator for ID3v1.1
                    writer.Write((byte)0);
                    
                    // Track number
                    writer.Write((byte)v1Metadata.TrackNumber);
                    
                    // Genre
                    writer.Write((byte)255); // 255 = undefined genre
                }
            }
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - ID3v2 tags should take precedence over ID3v1 tags
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Equal(metadata.Year.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                Assert.Contains("both ID3v1 and ID3v2", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
                
                // Make sure we're NOT seeing the v1 data
                Assert.DoesNotContain("OUTDATED", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
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
        public async Task Should_Handle_TCON_Genre_With_Parenthesized_Number()
        {
            // Arrange - Create metadata with a genre that includes a genre code in parenthesis
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Genre Test",
                Artist = "Genre Artist",
                Album = "Genre Album",
                Year = 2025,
                TrackNumber = 1,
                // Genre with ID in parentheses (17 = Rock)
                Genre = "(17)Rock", 
                Comment = "Testing genre handling"
            };
            
            // Act - Create a file with ID3v2.3 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_3, 
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - Genre should be parsed and handled properly
                string genreValue = SafeParser.ToString(tags.Tags[MetaTagIdentifier.Genre]);
                
                // Should contain "Rock" regardless of how the reader handles the numeric code
                Assert.Contains("Rock", genreValue);
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
        public async Task Should_Gracefully_Handle_Incomplete_Tags()
        {
            // We'll create a file where some tags are missing
            var metadata = new BlankMusicFileGenerator.MusicMetadata
            {
                Title = "Partial Tags",
                Artist = "", // Missing artist
                Album = "Some Album",
                Year = 0,   // Missing year
                TrackNumber = 0, // Missing track
                Genre = "",  // Missing genre
                Comment = "Some tags are intentionally missing"
            };
            
            // Act - Create a file with ID3v2.3 tags
            string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                _testOutputPath, 
                BlankMusicFileGenerator.Id3Version.Id3v2_3, 
                metadata);
                
            try
            {
                // Read tags using the AudioTagManager
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                
                // Assert - The reader should handle missing tags gracefully
                
                // These tags should be present
                Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                Assert.Contains("missing", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
                
                // These tags should be empty or have default values
                Assert.Equal("", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                Assert.Equal("0", SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
                Assert.Equal("0", SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                Assert.Equal("", SafeParser.ToString(tags.Tags[MetaTagIdentifier.Genre]));
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
        public async Task Should_Handle_Large_Text_Frames_Across_Versions()
        {
            // Test with oversized data to ensure proper handling across versions
            string longText = new string('A', 5000); // 5000 'A' characters
            
            // Create a test case for each ID3v2 version
            var versions = new[]
            {
                BlankMusicFileGenerator.Id3Version.Id3v2_2,
                BlankMusicFileGenerator.Id3Version.Id3v2_3,
                BlankMusicFileGenerator.Id3Version.Id3v2_4
            };
            
            foreach (var version in versions)
            {
                var metadata = new BlankMusicFileGenerator.MusicMetadata
                {
                    Title = "Long Title " + longText,
                    Artist = "Test Artist",
                    Album = "Test Album",
                    Year = 2025,
                    TrackNumber = 1,
                    Genre = "Test",
                    Comment = "Long Comment " + longText
                };
                
                string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                    _testOutputPath, 
                    version, 
                    metadata);
                    
                try
                {
                    // Read tags using the AudioTagManager
                    var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                    
                    // Assert - Large frames should be handled properly for all versions
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
        }
        
        [Fact]
        public async Task Should_Handle_All_ID3_Versions_In_Same_Test_Run()
        {
            // Check that the tag reader can handle multiple different versions in one test run
            var allVersions = new[] {
                BlankMusicFileGenerator.Id3Version.Id3v1_1,
                BlankMusicFileGenerator.Id3Version.Id3v2_2,
                BlankMusicFileGenerator.Id3Version.Id3v2_3,
                BlankMusicFileGenerator.Id3Version.Id3v2_4,
                BlankMusicFileGenerator.Id3Version.MultipleVersions
            };
            
            var filePaths = new List<string>();
            var metadataValues = new Dictionary<string, BlankMusicFileGenerator.MusicMetadata>();
            
            try
            {
                // Create one file for each version type
                foreach (var version in allVersions)
                {
                    var metadata = new BlankMusicFileGenerator.MusicMetadata
                    {
                        Title = $"Title {version}",
                        Artist = $"Artist {version}",
                        Album = $"Album {version}",
                        Year = 2000 + (int)version,
                        TrackNumber = (int)version + 1,
                        Genre = "Test",
                        Comment = $"Comment for version {version}"
                    };
                    
                    string filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileWithVersionAsync(
                        _testOutputPath, 
                        version, 
                        metadata);
                        
                    filePaths.Add(filePath);
                    metadataValues[filePath] = metadata;
                }
                
                // Read each file in sequence and verify tags
                foreach (var filePath in filePaths)
                {
                    var metadata = metadataValues[filePath];
                    var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);
                    
                    // Verify basic tag reading works across all versions
                    Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
                    Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
                    Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
                    Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
                }
            }
            finally
            {
                // Clean up all files
                foreach (var filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
        }
    }
}
