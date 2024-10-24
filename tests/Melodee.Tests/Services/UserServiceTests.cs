using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using NodaTime;
using Npgsql.Replication.PgOutput.Messages;

namespace Melodee.Tests.Services;

public sealed class UserServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListUsersAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.Add(new User
            {
                ApiKey = shouldContainApiKey,
                UserName = "Test User",
                Email = "testemail@local.lan",
                PasswordHash = "hopefully_a_good_password".ToPasswordHash(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        
        var service = GetUserService();
        var listResult = await service.ListAsync(ServiceUser.Instance.Value, new PagedRequest());
        Assert.NotNull(listResult);
        Assert.NotEmpty(listResult.Data);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
        
    }
}
