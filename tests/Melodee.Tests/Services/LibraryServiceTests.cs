using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Services;
using NodaTime;

namespace Melodee.Tests.Services;

public sealed class LibraryServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListLibrariesAsync()
    {
        var listResult = await GetLibraryService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Type == (int)LibraryType.Library);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(3, listResult.TotalCount);
    }
}
