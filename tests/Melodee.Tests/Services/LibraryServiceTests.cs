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
        var shouldContainApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Libraries.Add(new Library
            {
                ApiKey = shouldContainApiKey,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Name = "Staging",
                Path = "/home/steven/incoming/melodee_test/staging/",
                Type = (int)LibraryType.Staging,
            });
            await context.SaveChangesAsync();
        }
        var listResult = await GetLibraryService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }
}
