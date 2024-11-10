using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
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
                UserNameNormalized = "Test User".ToUpperInvariant(),
                Email = "testemail@local.lan",
                EmailNormalized = "testemail@local.lan".ToUpperInvariant(),
                PasswordHash = "hopefully_a_good_password".ToPasswordHash(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        var listResult = await GetUserService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }
    
    [Fact]
    public async Task ListUsersWithFilterEqualsAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.AddRange(new User
            {
                ApiKey = shouldContainApiKey,
                UserName = "Test User",
                UserNameNormalized = "Test User".ToUpperInvariant(),
                Email = "testemail@local.lan",
                EmailNormalized = "testemail@local.lan".ToUpperInvariant(),
                PasswordHash = "hopefully_a_good_password".ToPasswordHash(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            },
            new User
            {
                ApiKey = Guid.NewGuid(),
                UserName = "Test User2",
                UserNameNormalized = "Test User2".ToUpperInvariant(),
                Email = "testemail2@local.lan",
                EmailNormalized = "testemail2@local.lan".ToUpperInvariant(),
                PasswordHash = "hopefully_a_good_password2".ToPasswordHash(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        var listResult = await GetUserService().ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = 1,
            FilterBy = 
            [
                new FilterOperatorInfo(nameof(User.Email), FilterOperator.Equals, "testemail@local.lan")
            ]
        });
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }    

    [Fact]
    public async Task ListUsersWithFilterContainsAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.AddRange(new User
                {
                    ApiKey = shouldContainApiKey,
                    UserName = "Test User",
                    UserNameNormalized = "Test User".ToUpperInvariant(),
                    Email = "testemail@local.lan",
                    EmailNormalized = "testemail@local.lan".ToUpperInvariant(),
                    PasswordHash = "hopefully_a_good_password".ToPasswordHash(),
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                },
                new User
                {
                    ApiKey = Guid.NewGuid(),
                    UserName = "Test User2",
                    UserNameNormalized = "Test User2".ToUpperInvariant(),
                    Email = "bingobango@local.lan",
                    EmailNormalized = "bingobango@local.lan".ToUpperInvariant(),
                    PasswordHash = "hopefully_a_good_password2".ToPasswordHash(),
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                });
            await context.SaveChangesAsync();
        }
        var listResult = await GetUserService().ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = 1,
            FilterBy = 
            [
                new FilterOperatorInfo(nameof(User.Email), FilterOperator.Contains, "testemail")
            ]
        });
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

        // best I can figure out SQLite doesn't like sharing memory connections.
        var sleepTime = 200;

        var userService = GetUserService();
        var registerResult = await userService.RegisterAsync(emailAddress, emailAddress, emailAddress.ToPasswordHash());
        AssertResultIsSuccessful(registerResult);
        Assert.Equal(emailAddress, registerResult.Data!.Email);
        
        Thread.Sleep(sleepTime);
        
        // Register a second user to ensure that only the fist gets deleted
        var registerResult2 = await userService.RegisterAsync(emailAddress2, emailAddress2, emailAddress2.ToPasswordHash());
        AssertResultIsSuccessful(registerResult2);
        Assert.Equal(emailAddress2, registerResult2.Data!.Email);

        Thread.Sleep(sleepTime);
        
        var userByEmailAddress = await GetUserService().GetByEmailAddressAsync(emailAddress);
        AssertResultIsSuccessful(userByEmailAddress);
        Assert.Equal(emailAddress, userByEmailAddress.Data!.Email);
        
        Thread.Sleep(sleepTime);
        
        var authResult = await userService.LoginUserAsync(emailAddress, emailAddress.ToPasswordHash());
        AssertResultIsSuccessful(authResult);
        Assert.Equal(emailAddress, authResult.Data!.Email);
        
        Thread.Sleep(sleepTime);
        
        Assert.NotEqual(userByEmailAddress.Data.LastLoginAt, authResult.Data!.LastLoginAt);
        
        var deleteResult = await userService.DeleteAsync(ServiceUser.Instance.Value, userByEmailAddress.Data.ApiKey);
        AssertResultIsSuccessful(deleteResult);
        Assert.True(deleteResult.Data);
        
        Thread.Sleep(sleepTime);
        
        userByEmailAddress = await userService.GetByEmailAddressAsync(emailAddress);
        Assert.NotNull(userByEmailAddress);
        Assert.False(userByEmailAddress.IsSuccess);
        Assert.Null(userByEmailAddress.Data);
        
        Thread.Sleep(sleepTime);
        
        userByEmailAddress = await userService.GetByEmailAddressAsync(emailAddress2);
        AssertResultIsSuccessful(userByEmailAddress);
        Assert.Equal(emailAddress2, userByEmailAddress.Data!.Email);
        
        Thread.Sleep(sleepTime);
        
        var listResult = await userService.ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.Email == emailAddress2);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);        
    }
}
