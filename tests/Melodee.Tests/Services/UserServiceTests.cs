using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using NodaTime;

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
        var listResult = await service.ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }

    [Fact]
    public async Task RegisterGetAuthenticateAndDeleteUserAsync()
    {
        var emailAddress = "testemail@local.lan";
        var emailAddress2 = "testemail2@local.lan";
        
        var service = GetUserService();
        var registerResult = await service.RegisterAsync(emailAddress, emailAddress, emailAddress.ToPasswordHash());
        AssertResultIsSuccessful(registerResult);
        Assert.Equal(emailAddress, registerResult.Data!.Email);
        
        // Register a second user to ensure that only the fist gets deleted
        var registerResult2 = await service.RegisterAsync(emailAddress2, emailAddress2, emailAddress2.ToPasswordHash());
        AssertResultIsSuccessful(registerResult2);
        Assert.Equal(emailAddress2, registerResult2.Data!.Email);
        
        var userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress);
        AssertResultIsSuccessful(userByEmailAddress);
        Assert.Equal(emailAddress, userByEmailAddress.Data!.Email);
        
        var authResult = await service.LoginUserAsync(emailAddress, emailAddress.ToPasswordHash());
        AssertResultIsSuccessful(authResult);
        Assert.Equal(emailAddress, authResult.Data!.Email);
        
        Assert.NotEqual(userByEmailAddress.Data.LastLoginAt, authResult.Data!.LastLoginAt);
        
        var deleteResult = await service.DeleteAsync(ServiceUser.Instance.Value, userByEmailAddress.Data.ApiKey);
        AssertResultIsSuccessful(deleteResult);
        Assert.True(deleteResult.Data);
        
        userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress);
        Assert.NotNull(userByEmailAddress);
        Assert.False(userByEmailAddress.IsSuccess);
        Assert.Null(userByEmailAddress.Data);
        
        userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress2);
        AssertResultIsSuccessful(userByEmailAddress);
        Assert.Equal(emailAddress2, userByEmailAddress.Data!.Email);
        
        var listResult = await service.ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Email == emailAddress2);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);        
    }
}
