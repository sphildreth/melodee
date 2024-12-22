using System.Globalization;
using System.Net.Mail;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Utility;
using NodaTime;
using Album = Melodee.Common.Data.Models.Album;
using User = Melodee.Common.Data.Models.User;

namespace Melodee.Tests.Services;

public class OpenSubsonicApiServiceTests : ServiceTestBase
{
    [Fact]
    public async Task GetLicense()
    {
        var username = "daUsername";
        var password = "daPassword";
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            context.Users.Add(new User
            {
                ApiKey = Guid.NewGuid(),
                UserName = username,
                UserNameNormalized = username.ToUpperInvariant(),
                Email = "testemail@local.lan",
                EmailNormalized = "testemail@local.lan".ToNormalizedString()!,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, password, usersPublicKey),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        var licenseResult = await GetOpenSubsonicApiService().GetLicenseAsync(GetApiRequest(username, "123456", password));
        Assert.NotNull(licenseResult);
        Assert.True(licenseResult.IsSuccess);
        Assert.NotNull(licenseResult.ResponseData);
        var license = licenseResult.ResponseData?.Data as License;
        Assert.NotNull(license);
        Assert.True(DateTime.Parse(license.LicenseExpires, CultureInfo.InvariantCulture) > DateTime.Now);
    }
    
    [Fact]
    public async Task AuthenticateUserUsingSaltAndPassword()
    {
        var username = "daUsername";
        var password = "daPassword";
        var salt = "123487";
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            context.Users.Add(new User
            {
                ApiKey = Guid.NewGuid(),
                UserName = username,
                UserNameNormalized = username.ToUpperInvariant(),
                Email = "testemail@local.lan",
                EmailNormalized = "testemail@local.lan".ToNormalizedString()!,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(TestsBase.NewPluginsConfiguration().GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, password, usersPublicKey),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
        var authResult = await GetOpenSubsonicApiService().AuthenticateSubsonicApiAsync(GetApiRequest(username, salt, HashHelper.CreateMd5($"{password}{salt}") ?? string.Empty));
        Assert.NotNull(authResult);
        Assert.True(authResult.IsSuccess);
        Assert.NotNull(authResult.ResponseData);
    }    
    
}
