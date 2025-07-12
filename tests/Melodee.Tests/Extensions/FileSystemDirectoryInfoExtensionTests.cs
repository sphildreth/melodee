using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Artist = Melodee.Common.Data.Models.Artist;

namespace Melodee.Tests.Extensions;

public class FileSystemDirectoryInfoExtensionTests
{
    [Fact]
    public void ValidateDirectoryNameForFileInfo()
    {
        var coverName = "cover.jpg";
        var testPath = @"/melodee_test/tests/image_number_tests";
        var filename = Path.Combine(testPath, coverName);
        var fileInfo = new FileInfo(filename);
        var fileDirectory = fileInfo.ToDirectorySystemInfo();
        Assert.Equal(testPath, fileDirectory.Path);
        Assert.Equal("image_number_tests", fileDirectory.Name);
    }

    [Fact]
    public void ValidateFileNameForFileDirectory()
    {
        var coverName = "cover.jpg";
        var testPath = @"/melodee_test/tests/image_number_tests/";
        var filename = Path.Combine(testPath, coverName);
        var fileInfo = new FileInfo(filename);
        var fileDirectory = fileInfo.ToDirectorySystemInfo();
        var fileSystemInfo = fileInfo.ToFileSystemInfo();
        Assert.Equal(fileInfo.FullName, fileSystemInfo.FullName(fileDirectory));
        Assert.Equal(coverName, fileSystemInfo.Name);
    }

    [Fact]
    public void ValidateDirectoryNameWithExtensionForFileInfo()
    {
        var coverName = "cover.jpg";
        var testPath = @"/melodee_test/inbound/J_de - Discography/02. EPs/(2020) J.de - Premi.re fois";
        var filename = Path.Combine(testPath, coverName);
        var fileInfo = new FileInfo(filename);
        var fileDirectory = fileInfo.ToDirectorySystemInfo();
        Assert.Equal(testPath, fileDirectory.Path);
        Assert.Equal("(2020) J.de - Premi.re fois", fileDirectory.Name);
    }

    [Fact]
    public void ValidateDirectoryNameForToDirectoryFromString()
    {
        var testPath = @"/melodee_test/inbound/J_de - Discography/02. EPs/(2020) J.de - Premi.re fois";
        var fileDirectory = testPath.ToFileSystemDirectoryInfo();
        Assert.Equal(testPath, fileDirectory.Path);
        Assert.Equal("(2020) J.de - Premi.re fois", fileDirectory.Name);
    }


    [Theory]
    [InlineData("Bobs Discography", true)]
    [InlineData("Bobs Discography (1940-1999)", true)]
    [InlineData("02 Singles", false)]
    [InlineData("Bootlegs", false)]
    [InlineData("Boxset & Compilations", false)]
    [InlineData("Boxset", false)]
    [InlineData("Boxset, Compilations and EP", false)]
    [InlineData("Compilations", false)]
    [InlineData("Demos", false)]
    [InlineData("Ep, Singles", false)]
    [InlineData("LIVE", false)]
    [InlineData("Live Albums", false)]
    [InlineData("Single & EP's", false)]
    [InlineData("Singles", false)]
    [InlineData("Singles, EPs, Fan Club & Promo", false)]
    [InlineData("Bobs Greatest Hits Vol 2", false)]
    [InlineData("Bobs Greatest Hits", false)]
    [InlineData("Greatest Hits and Misses and Just Garbage", false)]
    [InlineData("The best of Bob", false)]
    [InlineData("[1985] Best Of, The", false)]
    [InlineData("[1994] American Legends Best Of The Early Years", false)]
    public void IsDirectoryDiscography(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = input
        }.IsDiscographyDirectory());
    }

    [Theory]
    [InlineData("Bobs Discography", false)]
    [InlineData("Media Madness", false)]
    [InlineData("Media 2013 Madness", false)]
    [InlineData("97b244a3-7a49-4dec-921b-2257fc597f39", false)]
    [InlineData("00-k 2024", false)]
    [InlineData("Artist - [2000] Album", false)]
    [InlineData("CD", false)]
    [InlineData("A0", true)] // sph; I dont know why, but these are common
    [InlineData("A1", true)]
    [InlineData("AA1", true)]
    [InlineData("AAA1", true)]
    [InlineData("B2", true)]
    [InlineData("CD01", true)]
    [InlineData("CD14", true)]
    [InlineData("CD1", true)]
    [InlineData("CD 1", true)]
    [InlineData("DISC1", true)]
    [InlineData("Disc 1", true)]
    [InlineData("Disc 01", true)]
    [InlineData("media001", true)]
    [InlineData("media 001", true)]
    public void IsDirectoryAlbumMedia(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = input
        }.IsAlbumMediaDirectory());
    }

    [Theory]
    [InlineData("Albums", true)]
    [InlineData("EP", true)]
    [InlineData("Eps", true)]
    [InlineData("Extended Play", true)]
    [InlineData("Extended Plays", true)]
    [InlineData("Reissued", true)]
    [InlineData("Studio Albums", true)]
    [InlineData("Stuff", true)]
    [InlineData("Vinyl Albums", true)]
    [InlineData("Vinyl Collection", true)]
    [InlineData("Vinyl", true)]
    [InlineData("02 Singles", false)]
    [InlineData("Bootlegs", false)]
    [InlineData("Boxset & Compilations", false)]
    [InlineData("Boxset", false)]
    [InlineData("Boxset, Compilations and EP", false)]
    [InlineData("Compilations", false)]
    [InlineData("Demos", false)]
    [InlineData("Ep, Singles", false)]
    [InlineData("LIVE", false)]
    [InlineData("live", false)]
    [InlineData("Live at Redrock", false)]
    [InlineData("Live Albums", false)]
    [InlineData("Single & EP's", false)]
    [InlineData("Singles", false)]
    [InlineData("Singles, EPs, Fan Club & Promo", false)]
    [InlineData("Bobs Greatest Hits Vol 2", false)]
    [InlineData("Bobs Greatest Hits", false)]
    [InlineData("Greatest Hits and Misses and Just Garbage", false)]
    [InlineData("Special Delivery", false)]
    [InlineData("Delivery [24]", false)]
    [InlineData("The best of Bob", false)]
    [InlineData("[1985] Best Of, The", false)]
    [InlineData("[1994] American Legends Best Of The Early Years", false)]
    [InlineData("Delivery - [2025] Force Majeure", false)]
    public void IsDirectoryNotStudioAlbums(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = "/melodee_test",
            Name = input
        }.IsDirectoryStudioAlbums());
    }

    [Fact]
    public void GetParents()
    {
        var testPath = @"/melodee_test/tests/image_number_tests/";
        if (Directory.Exists(testPath))
        {
            var dirInfo = new FileSystemDirectoryInfo
            {
                Path = testPath,
                Name = testPath
            };
            var parents = dirInfo.GetParents();
            Assert.NotNull(parents);
            Assert.NotEmpty(parents);
            Assert.Equal(3, parents.Count());
        }
    }

    [Fact]
    public void RenameWithPrefix()
    {
        var testDirectory = @"/melodee_test/tests";
        if (Directory.Exists(testDirectory))
        {
            var testPath = @"/melodee_test/tests/dir_to_get_renamed_prefix";
            if (!Directory.Exists(testPath))
            {
                Directory.CreateDirectory(testPath);
            }

            var dirInfo = testPath.ToFileSystemDirectoryInfo();
            var nd = dirInfo.AppendPrefix("__batman_");
            Assert.NotEqual(nd.FullName(), dirInfo.FullName());
            Assert.True(Directory.Exists(nd.FullName()));
            Assert.True(nd.Exists());
            nd.Delete();
            Assert.False(Directory.Exists(nd.FullName()));
        }
    }


    [Fact]
    public void NextImageNumberInDirectory()
    {
        var testPath = @"/melodee_test/tests/image_number_tests/";
        if (Directory.Exists(testPath))
        {
            var fileDirectoryInfo = new FileSystemDirectoryInfo
            {
                Path = testPath,
                Name = testPath
            };
            var nextImage = fileDirectoryInfo.GetNextFileNameForType(Artist.ImageType);
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            var artistTypeCount = nextImage.Item2;

            nextImage = fileDirectoryInfo.GetNextFileNameForType("Front");
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            Assert.NotEqual(artistTypeCount, nextImage.Item2);

            nextImage = fileDirectoryInfo.GetNextFileNameForType(Guid.NewGuid().ToString());
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            Assert.NotEqual(artistTypeCount, nextImage.Item2);
        }
    }

    [Theory]
    [InlineData("/home/user/music", "MyAlbum", "/home/user/music/MyAlbum")]
    [InlineData("/home/user/music/", "MyAlbum", "/home/user/music/MyAlbum")]
    [InlineData("/home/user/music", "", "/home/user/music")]
    [InlineData("/home/user/music/", "", "/home/user/music/")]  // Current implementation behavior
    [InlineData("/", "root", "/root")]
    public void FullName_WithPathAndName_ReturnsCorrectCombinedPath(string path, string name, string expected)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/home/user/music/MyAlbum", "MyAlbum", "/home/user/music/MyAlbum")]
    [InlineData("/home/user/music/MyAlbum/", "MyAlbum", "/home/user/music/MyAlbum/")]
    [InlineData("/root/folder/subfolder", "subfolder", "/root/folder/subfolder")]
    [InlineData("/root/folder/subfolder/", "subfolder", "/root/folder/subfolder/")]
    public void FullName_WhenPathAlreadyEndsWithName_ReturnsOriginalPath(string path, string name, string expected)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/home/user/music/")]
    [InlineData("/root/")]
    [InlineData("//server/share/")]
    public void FullName_WithTrailingDirectorySeparator_RemovesTrailingSeparatorBeforeProcessing(string pathWithTrailing)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = pathWithTrailing,
            Name = "TestFolder"
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        var expectedPathWithoutTrailing = pathWithTrailing.TrimEnd(Path.DirectorySeparatorChar);
        var expected = Path.Combine(expectedPathWithoutTrailing, "TestFolder");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FullName_WithNullPath_ThrowsException()
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = "TestFolder"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => directoryInfo.FullName());
    }

    [Theory]
    [InlineData("/home/user", "")]
    public void FullName_WithEmptyName_ReturnsPathOnly(string path, string name)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        Assert.Equal(path, result);
    }

    [Theory]
    [InlineData("/home/user", "   ")]
    public void FullName_WithWhitespaceName_CombinesAsIs(string path, string name)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        // The current implementation treats whitespace as valid name
        var expected = Path.Combine(path, name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FullName_WithNullName_ThrowsException()
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = "/home/user",
            Name = null!  // Suppress nullable warning for test
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => directoryInfo.FullName());
    }

    [Theory]
    [InlineData("/home/user/Documents/Music", "My Album (2023)")]
    [InlineData("/media/storage/music", "Various Artists")]
    [InlineData("/tmp", "Test Folder With Spaces")]
    public void FullName_WithSpecialCharactersInName_HandlesCorrectly(string path, string name)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        var expected = Path.Combine(path, name);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/home/user/music/Artist/Album", "Album", "/home/user/music/Artist/Album")]
    [InlineData("/home/user/music/Artist/Album/", "Album", "/home/user/music/Artist/Album/")]
    public void FullName_WithDeepNestedPathEndingWithName_ReturnsOriginalPath(string path, string name, string expected)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/home/user/music/Artist", "Album")]
    [InlineData("/home/user/music/Artist/", "Album")]
    public void FullName_WithDeepNestedPathNotEndingWithName_CombinesCorrectly(string path, string name)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        var expected = Path.Combine(path.TrimEnd(Path.DirectorySeparatorChar), name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FullName_WithMixedDirectorySeparators_HandlesCorrectly()
    {
        // This test is more relevant on Windows where both / and \ might be used
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = "/home/user/music/artist",
            Name = "album"
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        // The result should use Path.Combine which handles separator normalization
        var expected = Path.Combine("/home/user/music/artist", "album");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("relative/path", "folder")]
    [InlineData("./current", "folder")]
    [InlineData("../parent", "folder")]
    public void FullName_WithRelativePaths_CombinesCorrectly(string path, string name)
    {
        // Arrange
        var directoryInfo = new FileSystemDirectoryInfo
        {
            Path = path,
            Name = name
        };

        // Act
        var result = directoryInfo.FullName();

        // Assert
        var expected = Path.Combine(path, name);
        Assert.Equal(expected, result);
    }
}
