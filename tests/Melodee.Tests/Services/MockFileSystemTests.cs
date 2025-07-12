using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Services;

public class MockFileSystemTests : IDisposable
{
    
    private readonly MockFileSystemService _mockFileSystem = new();
    
    [Fact]
    public void MockFileSystem_EnumerateFiles_ReturnsCorrectFiles()
    {
        // Arrange
        var mock = FileSystemTestHelper.CreateTypicalAlbumStructure();
        var albumPath = "/music/Artist Name/Album Name (2023)";

        // Act
        var files = mock.EnumerateFiles(albumPath, "*.mp3", SearchOption.TopDirectoryOnly);

        // Assert
        Assert.Equal(3, files.Count());
        Assert.Contains("/music/Artist Name/Album Name (2023)/01 - Track One.mp3", files);
        Assert.Contains("/music/Artist Name/Album Name (2023)/02 - Track Two.mp3", files);
        Assert.Contains("/music/Artist Name/Album Name (2023)/03 - Track Three.mp3", files);
    }

    [Fact]
    public void MockFileSystem_DirectoryExists_WorksCorrectly()
    {
        // Arrange
        var mock = FileSystemTestHelper.CreateTypicalAlbumStructure();

        // Act & Assert
        Assert.True(mock.DirectoryExists("/music"));
        Assert.True(mock.DirectoryExists("/music/Artist Name"));
        Assert.True(mock.DirectoryExists("/music/Artist Name/Album Name (2023)"));
        Assert.False(mock.DirectoryExists("/music/NonExistent"));
    }

    [Fact]
    public void MockFileSystem_DeleteDirectory_RemovesDirectory()
    {
        // Arrange
        var mock = FileSystemTestHelper.CreateTypicalAlbumStructure();
        var albumPath = "/music/Artist Name/Album Name (2023)";

        // Act
        mock.DeleteDirectory(albumPath, false);

        // Assert
        Assert.False(mock.DirectoryExists(albumPath));
        Assert.True(mock.DirectoryExists("/music/Artist Name")); // Parent still exists
    }

    [Fact]
    public async Task MockFileSystem_DeserializeAlbumAsync_ReturnsSetupAlbum()
    {
        // Arrange
        var testAlbum = FileSystemTestHelper.CreateSampleAlbum();
        var filePath = "/music/test/melodee.json";
        
        _mockFileSystem.SetAlbumForFile(filePath, testAlbum);

        // Act
        var result = await _mockFileSystem.DeserializeAlbumAsync(filePath, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testAlbum.Id, result.Id);
        Assert.Equal(testAlbum.AlbumTitle(), result.AlbumTitle());
    }
    
    public void Dispose()
    {
        _mockFileSystem.Reset();
    }    
}
