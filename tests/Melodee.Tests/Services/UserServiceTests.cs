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
        var listResult = await service.ListAsync(ServiceUser.Instance.Value, new PagedRequest());
        Assert.NotNull(listResult);
        Assert.True(listResult.IsSuccess); 
        Assert.NotEmpty(listResult.Data);
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
        Assert.NotNull(registerResult);
        Assert.True(registerResult.IsSuccess);        
        Assert.NotNull(registerResult.Data);
        Assert.Equal(emailAddress, registerResult.Data.Email);
        
        // Register a second user to ensure that only the fist gets deleted
        var registerResult2 = await service.RegisterAsync(emailAddress2, emailAddress2, emailAddress2.ToPasswordHash());
        Assert.NotNull(registerResult2);
        Assert.True(registerResult2.IsSuccess);        
        Assert.NotNull(registerResult2.Data);
        Assert.Equal(emailAddress2, registerResult2.Data.Email);

        
        var userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress);
        Assert.NotNull(userByEmailAddress);
        Assert.True(userByEmailAddress.IsSuccess);
        Assert.NotNull(userByEmailAddress.Data);
        Assert.Equal(emailAddress, userByEmailAddress.Data.Email);
        
        var authResult = await service.AuthenticateAsync(emailAddress, emailAddress.ToPasswordHash());
        Assert.NotNull(authResult);
        Assert.NotNull(authResult.Data);
        Assert.True(authResult.IsSuccess);
        Assert.Equal(emailAddress, authResult.Data.Email);
        
        var deleteResult = await service.DeleteAsync(ServiceUser.Instance.Value, userByEmailAddress.Data.ApiKey);
        Assert.NotNull(deleteResult);
        Assert.True(deleteResult.IsSuccess);
        Assert.True(deleteResult.Data);
        
        userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress);
        Assert.NotNull(userByEmailAddress);
        Assert.False(userByEmailAddress.IsSuccess);
        Assert.Null(userByEmailAddress.Data);
        
        userByEmailAddress = await service.GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress2);
        Assert.NotNull(userByEmailAddress);
        Assert.True(userByEmailAddress.IsSuccess);
        Assert.NotNull(userByEmailAddress.Data);
        Assert.Equal(emailAddress2, userByEmailAddress.Data.Email);
        
        var listResult = await service.ListAsync(ServiceUser.Instance.Value, new PagedRequest());
        Assert.NotNull(listResult);
        Assert.True(listResult.IsSuccess); 
        Assert.NotEmpty(listResult.Data);
        Assert.Contains(listResult.Data, x => x.Email == emailAddress2);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);        
    }
}
