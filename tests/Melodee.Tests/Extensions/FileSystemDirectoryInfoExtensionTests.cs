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
}
