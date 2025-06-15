using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Melodee.Common.Utility;
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
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            context.Users.Add(new User
            {
                ApiKey = shouldContainApiKey,
                UserName = "Test User",
                UserNameNormalized = "Test User".ToUpperInvariant(),
                Email = "testemail@local.lan",
                EmailNormalized = "testemail@local.lan".ToNormalizedString()!,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "hopefully_a_good_password", usersPublicKey),
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
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            var usersPublicKey2 = EncryptionHelper.GenerateRandomPublicKeyBase64();

            context.Users.AddRange(new User
                {
                    ApiKey = shouldContainApiKey,
                    UserName = "Test User",
                    UserNameNormalized = "Test User".ToUpperInvariant(),
                    Email = "testemail@local.lan",
                    EmailNormalized = "testemail@local.lan".ToNormalizedString()!,
                    PublicKey = usersPublicKey,
                    PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "hopefully_a_good_password", usersPublicKey),
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                },
                new User
                {
                    ApiKey = Guid.NewGuid(),
                    UserName = "Test User2",
                    UserNameNormalized = "Test User2".ToUpperInvariant(),
                    Email = "testemail2@local.lan",
                    EmailNormalized = "testemail2@local.lan".ToNormalizedString()!,
                    PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "hopefully_a_good_password2", usersPublicKey2),
                    PublicKey = usersPublicKey2,
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
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            var usersPublicKey2 = EncryptionHelper.GenerateRandomPublicKeyBase64();
            context.Users.AddRange(new User
                {
                    ApiKey = shouldContainApiKey,
                    UserName = "Test User",
                    UserNameNormalized = "Test User".ToUpperInvariant(),
                    Email = "testemail@local.lan",
                    EmailNormalized = "testemail@local.lan".ToNormalizedString()!,
                    PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "testemail@local.lan".ToNormalizedString() + "hopefully_a_good_password", usersPublicKey),
                    PublicKey = usersPublicKey,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
                },
                new User
                {
                    ApiKey = Guid.NewGuid(),
                    UserName = "Test User2",
                    UserNameNormalized = "Test User2".ToUpperInvariant(),
                    Email = "bango@local.lan",
                    EmailNormalized = "bango@local.lan".ToNormalizedString()!,
                    PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "hopefully_a_good_password2", usersPublicKey2),
                    PublicKey = usersPublicKey2,
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
        var registerResult = await userService.RegisterAsync(emailAddress, emailAddress, emailAddress, null);
        AssertResultIsSuccessful(registerResult);
        Assert.Equal(emailAddress, registerResult.Data!.Email);

        Thread.Sleep(sleepTime);

        // Register a second user to ensure that only the fist gets deleted
        var registerResult2 = await userService.RegisterAsync(emailAddress2, emailAddress2, emailAddress, null);
        AssertResultIsSuccessful(registerResult2);
        Assert.Equal(emailAddress2, registerResult2.Data!.Email);

        Thread.Sleep(sleepTime);

        var userByEmailAddress = await GetUserService().GetByEmailAddressAsync(emailAddress);
        AssertResultIsSuccessful(userByEmailAddress);
        Assert.Equal(emailAddress, userByEmailAddress.Data!.Email);

        Thread.Sleep(sleepTime);

        var authResult = await userService.LoginUserAsync(emailAddress, emailAddress);
        AssertResultIsSuccessful(authResult);
        Assert.Equal(emailAddress, authResult.Data!.Email);

        Thread.Sleep(sleepTime);

        Assert.NotEqual(userByEmailAddress.Data.LastLoginAt, authResult.Data!.LastLoginAt);

        var deleteResult = await userService.DeleteAsync([userByEmailAddress.Data.Id]);
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

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var username = "TestUsername";
        var apiKey = Guid.NewGuid();
        var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.Add(new User
            {
                ApiKey = apiKey,
                UserName = username,
                UserNameNormalized = username.ToUpperInvariant(),
                Email = "testuser@local.lan",
                EmailNormalized = "testuser@local.lan".ToNormalizedString()!,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "password", usersPublicKey),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        
        // Act
        var userService = GetUserService();
        var result = await userService.GetByUsernameAsync(username);
        
        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(username, result.Data.UserName);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }
    
    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Act
        var userService = GetUserService();
        var result = await userService.GetByUsernameAsync("NonExistentUser");
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotEmpty(result.Messages?.ToList() ?? new List<string>());
    }
    
    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUserData()
    {
        // Arrange
        var sleepTime = 200;
        var userService = GetUserService();
        
        // Register a user
        var registerResult = await userService.RegisterAsync("update@local.lan", "UpdateUser", "password123", null);
        AssertResultIsSuccessful(registerResult);
        var user = registerResult.Data!;
        
        Thread.Sleep(sleepTime);
        
        // Modify user properties
        user.UserName = "UpdatedUser";
        
        // Act - Update the user
        var updateResult = await userService.UpdateAsync(ServiceUser.NewServiceUser(), user);
        
        // Assert
        AssertResultIsSuccessful(updateResult);
        Assert.True(updateResult.Data);
        
        Thread.Sleep(sleepTime);
        
        // Verify update
        var userResult = await userService.GetAsync(user.Id);
        AssertResultIsSuccessful(userResult);
        Assert.Equal("UpdatedUser", userResult.Data!.UserName);
    }
    
    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var userService = GetUserService();
        var email = "duplicate@local.lan";
        
        // Register first user
        var firstRegisterResult = await userService.RegisterAsync(email, "FirstUser", "password123", null);
        AssertResultIsSuccessful(firstRegisterResult);
        
        // Act - Try to register with same email
        var duplicateRegisterResult = await userService.RegisterAsync(email, "SecondUser", "password456", null);
        
        // Assert
        Assert.False(duplicateRegisterResult.IsSuccess);
        Assert.Null(duplicateRegisterResult.Data);
        Assert.NotEmpty(duplicateRegisterResult.Messages?.ToList() ?? new List<string>());
    }
    
    // [Fact]
    // public async Task SaveProfileImageAsync_ShouldSaveImage()
    // {
    //     // Arrange
    //     var userService = GetUserService();
    //     var email = "profileimage@local.lan";
    //     
    //     // Create a user to test with
    //     var registerResult = await userService.RegisterAsync(email, "ImageUser", "password", null);
    //     AssertResultIsSuccessful(registerResult);
    //     var userId = registerResult.Data!.Id;
    //     
    //     // Create a simple test image (a small black GIF)
    //     byte[] testImageBytes = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B };
    //     
    //     // Act
    //     var saveImageResult = await userService.SaveProfileImageAsync(userId, testImageBytes);
    //     
    //     // Assert
    //     AssertResultIsSuccessful(saveImageResult);
    //     Assert.True(saveImageResult.Data);
    // }
    
    [Fact]
    public async Task SaveProfileImageAsync_ShouldReturnFailure_ForInvalidUserId()
    {
        // Arrange
        var userService = GetUserService();
        var invalidUserId = 9999;
        byte[] testImageBytes = { 0x47, 0x49, 0x46, 0x38 }; // Small image data
        
        // Act
        var saveImageResult = await userService.SaveProfileImageAsync(invalidUserId, testImageBytes);
        
        // Assert
        Assert.False(saveImageResult.IsSuccess);
        Assert.False(saveImageResult.Data);
        Assert.NotEmpty(saveImageResult.Messages?.ToList() ?? new List<string>());
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldFailForNonExistingUser()
    {
        // Arrange
        var userService = GetUserService();
        var nonExistingUserId = 9999;
        
        // Act
        var deleteResult = await userService.DeleteAsync([nonExistingUserId]);
        
        // Assert
        Assert.False(deleteResult.IsSuccess);
        Assert.False(deleteResult.Data);
    }
    
    [Fact]
    public async Task GetByApiKeyAsync_ShouldReturnUser_WhenApiKeyExists()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Users.Add(new User
            {
                ApiKey = apiKey,
                UserName = "ApiKeyUser",
                UserNameNormalized = "APIKEYUSER",
                Email = "apikey@local.lan",
                EmailNormalized = "apikey@local.lan".ToNormalizedString()!,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, "password", usersPublicKey),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        
        // Act
        var userService = GetUserService();
        var result = await userService.GetByApiKeyAsync(apiKey);
        
        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }
    
    [Fact]
    public async Task LoginUserAsync_ShouldFailWithIncorrectPassword()
    {
        // Arrange
        var userService = GetUserService();
        var email = "badlogin@local.lan";
        var password = "correctPassword";
        
        // Register the user
        var registerResult = await userService.RegisterAsync(email, "BadLoginUser", password, null);
        AssertResultIsSuccessful(registerResult);
        
        // Act
        var loginResult = await userService.LoginUserAsync(email, "wrongPassword");
        
        // Assert
        Assert.False(loginResult.IsSuccess);
        Assert.Null(loginResult.Data);
        Assert.NotEmpty(loginResult.Messages?.ToList() ?? new List<string>());
    }
    
    
}
