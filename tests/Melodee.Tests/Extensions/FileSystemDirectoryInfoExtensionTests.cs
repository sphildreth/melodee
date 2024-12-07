using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;

namespace Melodee.Tests.Extensions;

public class FileSystemDirectoryInfoExtensionTests
{
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
    [InlineData("Live Albums", false)]
    [InlineData("Single & EP's", false)]
    [InlineData("Singles", false)]
    [InlineData("Singles, EPs, Fan Club & Promo", false)]
    [InlineData("Bobs Greatest Hits Vol 2", false)]
    [InlineData("Bobs Greatest Hits", false)]
    [InlineData("Greatest Hits and Misses and Just Garbage", false)]
    [InlineData("The best of Bob", false)]
    public void IsDirectoryNotStudioAlbums(string input, bool shouldBe)
    {
        Assert.Equal(shouldBe, new FileSystemDirectoryInfo
        {
            Path = string.Empty,
            Name = input
        }.IsDirectoryStudioAlbums());
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
