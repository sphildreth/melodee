using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class FileSystemDirectoryInfoExtensionTests
{
    [Theory]
    [InlineData("Albums", true)]
    [InlineData("Studio Albums", true)]
    [InlineData("Stuff", true)]
    [InlineData("Singles", false)]
    [InlineData("Demos", false)]
    [InlineData("02 Singles", false)]
    [InlineData("Compilations", false)]
    [InlineData("Live Albums", false)]
    public void IsDirectoryNotStudioAlbums(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = input
        }.IsDirectoryNotStudioAlbums());
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
            var nextImage = fileDirectoryInfo.GetNextFileNameForType(maxAllowed, Melodee.Common.Data.Models.Artist.ImageType);
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
