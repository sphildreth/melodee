using Melodee.Common.Enums;
using Melodee.Common.Models;
using NodaTime;

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
    /// Creates a mock file system with multiple artists and albums for complex testing scenarios.
    /// </summary>
    public static MockFileSystemService CreateMultiArtistStructure()
    {
        var mock = new MockFileSystemService();
        
        var rootPath = "/music";
        mock.SetDirectoryExists(rootPath);
        
        // First artist
        var artist1Path = "/music/Artist One";
        mock.SetDirectoryExists(artist1Path)
            .AddSubdirectory(rootPath, "Artist One");
            
        var album1Path = "/music/Artist One/First Album (2022)";
        mock.SetDirectoryExists(album1Path)
            .AddSubdirectory(artist1Path, "First Album (2022)")
            .AddFilesToDirectory(album1Path,
                "/music/Artist One/First Album (2022)/01 - Song A.mp3",
                "/music/Artist One/First Album (2022)/02 - Song B.mp3",
                "/music/Artist One/First Album (2022)/melodee.json");
        
        // Second artist
        var artist2Path = "/music/Artist Two";
        mock.SetDirectoryExists(artist2Path)
            .AddSubdirectory(rootPath, "Artist Two");
            
        var album2Path = "/music/Artist Two/Second Album (2023)";
        mock.SetDirectoryExists(album2Path)
            .AddSubdirectory(artist2Path, "Second Album (2023)")
            .AddFilesToDirectory(album2Path,
                "/music/Artist Two/Second Album (2023)/01 - Track X.mp3",
                "/music/Artist Two/Second Album (2023)/02 - Track Y.mp3",
                "/music/Artist Two/Second Album (2023)/melodee.json");
        
        return mock;
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
