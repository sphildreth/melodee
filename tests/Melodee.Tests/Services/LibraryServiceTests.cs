using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Services;

namespace Melodee.Tests.Services;

public sealed class LibraryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListLibrariesAsync()
    {
        var listResult = await MockLibraryService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Type == (int)LibraryType.Library);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(3, listResult.TotalCount);
    }
    
}
