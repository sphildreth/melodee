using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Artist = Melodee.Common.Data.Models.Artist;

namespace Melodee.Tests.Extensions;

public class FileSystemDirectoryInfoExtensionTests
{
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
    [InlineData("DISC1", true)]
    [InlineData("media001", true)]
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
    [InlineData("The best of Bob", false)]
    [InlineData("[1985] Best Of, The", false)]
    [InlineData("[1994] American Legends Best Of The Early Years", false)]
    public void IsDirectoryNotStudioAlbums(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = string.Empty,
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
        var testPath = @"/melodee_test/tests/dir_to_get_renamed_prefix";
        if (!Directory.Exists(testPath))
        {
            Directory.CreateDirectory(testPath);
        }

        var dirInfo = new FileSystemDirectoryInfo
        {
            Path = testPath,
            Name = "dir_to_get_renamed_prefix"
        };
        var nd = dirInfo.AppendPrefix("__batman_");
        Assert.NotEqual(nd.FullName(), dirInfo.FullName());
        Assert.True(Directory.Exists(nd.FullName()));
        Assert.True(nd.Exists());
        nd.Delete();
        Assert.False(Directory.Exists(nd.FullName()));
    }

    [Fact]
    public void ValidateMoveTo()
    {
        var testDirectory = @"/melodee_test/tests";
        if (Directory.Exists(testDirectory))
        {
            var createdDirName = Path.Combine(testDirectory, Guid.NewGuid().ToString());
            var createdDir = Directory.CreateDirectory(createdDirName);

            // Create a new file in dir to move
            var createdFileName = Path.Combine(createdDirName, Guid.NewGuid().ToString());
            var createdFileInfo = new FileInfo(createdFileName);
            File.Create(createdFileName).Dispose();
            Assert.True(File.Exists(createdFileName));
            Assert.True(createdFileInfo.Exists);

            // Move directory which should move file in directory as well 
            var directorySystemInfo = createdDir.ToDirectorySystemInfo();
            var movedToDirName = Path.Combine(testDirectory, Guid.NewGuid().ToString());
            directorySystemInfo.MoveToDirectory(movedToDirName);
            Assert.False(Directory.Exists(createdDirName));
            Assert.True(Directory.Exists(movedToDirName));

            // Ensure file got moved with directory
            var movedCreatedFileName = Path.Combine(movedToDirName, createdFileInfo.Name);
            Assert.True(File.Exists(movedCreatedFileName));

            var directoryInfo = new DirectoryInfo(movedToDirName);
            Assert.Equal(directoryInfo.Name, directorySystemInfo.Name);
            Assert.Equal(directoryInfo.FullName, directorySystemInfo.Path);
            Assert.Equal(directoryInfo.FullName, directorySystemInfo.FullName());
            Assert.True(directorySystemInfo.Exists());

            directorySystemInfo.Delete();
            Assert.False(Directory.Exists(movedToDirName));
            Assert.False(directorySystemInfo.Exists());
        }
    }

    [Fact]
    public void NextImageNumberInFolder()
    {
        var testPath = @"/melodee_test/tests/image_number_tests/";
        if (Directory.Exists(testPath))
        {
            var fileDirectoryInfo = new FileSystemDirectoryInfo
            {
                Path = testPath,
                Name = testPath
            };
            short maxAllowed = 2;
            var nextImage = fileDirectoryInfo.GetNextFileNameForType(maxAllowed, Artist.ImageType);
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            var artistTypeCount = nextImage.Item2;

            nextImage = fileDirectoryInfo.GetNextFileNameForType(maxAllowed, "Front");
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            Assert.NotEqual(artistTypeCount, nextImage.Item2);

            nextImage = fileDirectoryInfo.GetNextFileNameForType(maxAllowed, Guid.NewGuid().ToString());
            Assert.NotNull(nextImage.Item1);
            Assert.True(nextImage.Item2 > 0);
            Assert.NotEqual(artistTypeCount, nextImage.Item2);
        }
    }
}
