using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags;
using Melodee.Common.Utility;
using Melodee.Tests.Utility;

namespace Melodee.Tests.MetaData;

/// <summary>
///     Tests that demonstrate using blank music files for testing tag readers
/// </summary>
public class BlankMusicFileTests
{
    private readonly string _testOutputPath;

    public BlankMusicFileTests()
    {
        // Create a test directory in the current directory
        _testOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "BlankMusicTestFiles");
        Directory.CreateDirectory(_testOutputPath);
    }

    [Fact]
    public async Task Test_Generated_Mp3_With_Tag_Reader()
    {
        // Set up custom metadata
        var metadata = new BlankMusicFileGenerator.MusicMetadata
        {
            Title = "Mp3 Test Song",
            Artist = "Test Artist",
            Album = "Test Album",
            RecordingYear = 2025,
            TrackNumber = 3,
            Genre = "Electronic",
            Comment = "This is a test Mp3 file with ID3v2 tags but no audio data"
        };

        // Generate a minimal MP3 file
        var filePath = await BlankMusicFileGenerator.CreateMinimalMp3FileAsync(_testOutputPath, metadata);

        // Make sure the file exists
        Assert.True(File.Exists(filePath), "Generated Mp3 file should exist");

        try
        {
            // Read the tags using the AudioTagManager
            var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);

            // Verify the file format
            Assert.Equal(AudioFormat.MP3, tags.Format);

            // Verify that file metadata was populated
            Assert.NotEqual(0, tags.FileMetadata.FileSize);
            Assert.Equal(filePath, tags.FileMetadata.FilePath);
            Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.Created);
            Assert.NotEqual(DateTimeOffset.MinValue, tags.FileMetadata.LastModified);

            // Verify the tags match what we set
            Assert.NotNull(tags.Tags);
            Assert.NotEmpty(tags.Tags);

            // Check specific tag values
            Assert.Equal(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
            Assert.Equal(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
            Assert.Equal(metadata.Album, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Album]));
            Assert.Equal(metadata.RecordingYear.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.RecordingYear]));
            Assert.Equal(metadata.TrackNumber.ToString(), SafeParser.ToString(tags.Tags[MetaTagIdentifier.TrackNumber]));
            Assert.Contains(metadata.Comment, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Comment]));
        }
        finally
        {
            // Clean up
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task Test_Generated_Vorbis_With_Tag_Reader()
    {
        // Set up custom metadata
        var metadata = new BlankMusicFileGenerator.MusicMetadata
        {
            Title = "Vorbis Test Song",
            Artist = "Vorbis Tester",
            Album = "Test Collection",
            RecordingYear = 2025,
            TrackNumber = 5,
            Genre = "Test Genre",
            Comment = "This is a test Vorbis file with Vorbis comments but no audio data"
        };

        // Generate a minimal Vorbis file
        var filePath = BlankMusicFileGenerator.CreateMinimalVorbisFile(_testOutputPath, metadata);

        // Make sure the file exists
        Assert.True(File.Exists(filePath), "Generated Vorbis file should exist");

        try
        {
            // Read the tags using the AudioTagManager
            var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);

            // Verify the file format
            Assert.Equal(AudioFormat.Vorbis, tags.Format);

            // Verify that file metadata was populated
            Assert.NotEqual(0, tags.FileMetadata.FileSize);
            Assert.Equal(filePath, tags.FileMetadata.FilePath);

            // Verify tags were read
            Assert.NotNull(tags.Tags);
            Assert.NotEmpty(tags.Tags);

            // Check specific tag values that should be present
            Assert.Contains(metadata.Title, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]));
            Assert.Contains(metadata.Artist, SafeParser.ToString(tags.Tags[MetaTagIdentifier.Artist]));
        }
        finally
        {
            // Clean up
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task Test_Generate_Multiple_File_Types()
    {
        // Setup standard metadata with very distinct title that couldn't be confused
        var metadata = new BlankMusicFileGenerator.MusicMetadata
        {
            Title = "Multi-Format Test Title UNIQUE",
            Artist = "Test Suite",
            Album = "Test Batch",
            RecordingYear = 2025,
            TrackNumber = 1,
            Genre = "Test"
        };

        // Generate all supported file types at once
        var files = await BlankMusicFileGenerator.CreateTestFileBatchAsync(_testOutputPath, metadata);

        try
        {
            // Verify that files were created
            Assert.NotEmpty(files);

            foreach (var filePath in files)
            {
                // Verify each file exists
                Assert.True(File.Exists(filePath), $"Generated file should exist: {filePath}");

                // Read tags from each file
                var tags = await AudioTagManager.ReadAllTagsAsync(filePath, CancellationToken.None);

                // Verify format is not unknown
                Assert.NotEqual(AudioFormat.Unknown, tags.Format);

                // Verify tags were read
                Assert.NotNull(tags.Tags);
                Assert.NotEmpty(tags.Tags);

                // Get the title from the tags
                var tagTitle = SafeParser.ToString(tags.Tags[MetaTagIdentifier.Title]);

                // Check if the title contains the expected value - using Contains for more flexibility
                Assert.Contains(metadata.Title, tagTitle);
            }
        }
        finally
        {
            // Clean up
            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
