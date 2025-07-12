using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Tests.Services;

/// <summary>
/// Test helper class that provides pre-configured file system scenarios for testing AlbumDiscoveryService.
/// </summary>
public static class FileSystemTestHelper
{
    /// <summary>
    /// Creates a mock file system with a typical album directory structure.
    /// </summary>
    public static MockFileSystemService CreateTypicalAlbumStructure()
    {
        var mock = new MockFileSystemService();
        
        // Root music directory
        var rootPath = "/music";
        mock.SetDirectoryExists(rootPath);
        
        // Artist directory
        var artistPath = "/music/Artist Name";
        mock.SetDirectoryExists(artistPath)
            .AddSubdirectory(rootPath, "Artist Name");
        
        // Album directory
        var albumPath = "/music/Artist Name/Album Name (2023)";
        mock.SetDirectoryExists(albumPath)
            .AddSubdirectory(artistPath, "Album Name (2023)");
        
        // Add typical album files
        mock.AddFilesToDirectory(albumPath,
            "/music/Artist Name/Album Name (2023)/01 - Track One.mp3",
            "/music/Artist Name/Album Name (2023)/02 - Track Two.mp3",
            "/music/Artist Name/Album Name (2023)/03 - Track Three.mp3",
            "/music/Artist Name/Album Name (2023)/melodee.json",
            "/music/Artist Name/Album Name (2023)/cover.jpg"
        );
        
        // Set file creation times
        var baseTime = DateTime.UtcNow.AddDays(-30);
        mock.SetFileCreationTime("/music/Artist Name/Album Name (2023)/01 - Track One.mp3", baseTime)
            .SetFileCreationTime("/music/Artist Name/Album Name (2023)/02 - Track Two.mp3", baseTime.AddMinutes(1))
            .SetFileCreationTime("/music/Artist Name/Album Name (2023)/03 - Track Three.mp3", baseTime.AddMinutes(2))
            .SetFileCreationTime("/music/Artist Name/Album Name (2023)/melodee.json", baseTime.AddMinutes(5));
        
        return mock;
    }
    
    /// <summary>
    /// Creates a mock file system with multiple albums for testing bulk operations.
    /// </summary>
    public static MockFileSystemService CreateMultipleAlbumsStructure()
    {
        var mock = new MockFileSystemService();
        
        var rootPath = "/music";
        mock.SetDirectoryExists(rootPath);
        
        // Create multiple artists and albums
        var artists = new[] { "Artist A", "Artist B", "Various Artists" };
        var albums = new[] { "Album 1 (2020)", "Album 2 (2021)", "Compilation (2022)" };
        
        for (int i = 0; i < artists.Length; i++)
        {
            var artistPath = $"/music/{artists[i]}";
            mock.SetDirectoryExists(artistPath)
                .AddSubdirectory(rootPath, artists[i]);
            
            var albumPath = $"/music/{artists[i]}/{albums[i]}";
            mock.SetDirectoryExists(albumPath)
                .AddSubdirectory(artistPath, albums[i]);
            
            // Add files for each album
            mock.AddFilesToDirectory(albumPath,
                $"{albumPath}/01 - Song One.mp3",
                $"{albumPath}/02 - Song Two.mp3",
                $"{albumPath}/melodee.json"
            );
        }
        
        return mock;
    }
    
    /// <summary>
    /// Creates a mock file system with problematic scenarios for edge case testing.
    /// </summary>
    public static MockFileSystemService CreateProblematicStructure()
    {
        var mock = new MockFileSystemService();
        
        var rootPath = "/music";
        mock.SetDirectoryExists(rootPath);
        
        // Empty directory
        var emptyPath = "/music/Empty Artist";
        mock.SetDirectoryExists(emptyPath)
            .AddSubdirectory(rootPath, "Empty Artist");
        
        // Directory with only non-music files
        var nonMusicPath = "/music/Documents";
        mock.SetDirectoryExists(nonMusicPath)
            .AddSubdirectory(rootPath, "Documents")
            .AddFilesToDirectory(nonMusicPath,
                "/music/Documents/readme.txt",
                "/music/Documents/info.pdf"
            );
        
        // Album without melodee.json
        var incompleteAlbumPath = "/music/Incomplete Artist/Incomplete Album";
        mock.SetDirectoryExists("/music/Incomplete Artist")
            .SetDirectoryExists(incompleteAlbumPath)
            .AddSubdirectory(rootPath, "Incomplete Artist")
            .AddSubdirectory("/music/Incomplete Artist", "Incomplete Album")
            .AddFilesToDirectory(incompleteAlbumPath,
                "/music/Incomplete Artist/Incomplete Album/track.mp3"
            );
        
        return mock;
    }
    
    /// <summary>
    /// Creates a sample Album object for testing.
    /// </summary>
    public static Album CreateSampleAlbum(string artistName = "Test Artist", string albumName = "Test Album")
    {
        return new Album
        {
            Id = Guid.NewGuid(),
            Artist = new Artist(artistName,
                artistName,
                null),
            Directory = new FileSystemDirectoryInfo
            {
                Path = $"/music/{artistName}/{albumName}",
                Name = albumName
            },
            OriginalDirectory = new FileSystemDirectoryInfo
            {
                Path = $"/music/{artistName}/{albumName}",
                Name = albumName
            },
            ViaPlugins = ["TestPlugin"],
            Tags = new List<MetaTag<object?>>
            {
                new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = albumName },
                new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = artistName },
                new MetaTag<object?> { Identifier = MetaTagIdentifier.AlbumArtist, Value = artistName },
                new MetaTag<object?> { Identifier = MetaTagIdentifier.RecordingDate, Value = 2023 },
                new MetaTag<object?> { Identifier = MetaTagIdentifier.OrigAlbumYear, Value = 2023 }
            },
        };
    }
    
    /// <summary>
    /// Sets up a mock file system with a specific album and its melodee.json file.
    /// </summary>
    public static MockFileSystemService SetupAlbumWithMelodeeFile(this MockFileSystemService mock, Album album)
    {
        var albumPath = album.Directory.Path;
        var melodeeFilePath = Path.Combine(albumPath, "melodee.json");
        
        mock.SetDirectoryExists(albumPath)
            .AddFilesToDirectory(albumPath, melodeeFilePath)
            .SetAlbumForFile(melodeeFilePath, album);
        
        return mock;
    }
}
