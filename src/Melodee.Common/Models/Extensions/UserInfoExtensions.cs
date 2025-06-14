using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class UserInfoExtensions
{
    public static string Decrypt(this UserInfo user, string encryptedText, IMelodeeConfiguration configuration)
    {
        return EncryptionHelper.Decrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
            encryptedText, user.PublicKey);
    }

    public static string ToAvatarFileName(this UserInfo user, string libraryPath)
    {
        return Path.Combine(libraryPath, $"{user.Id.ToStringPadLeft(8)}.gif");
    }

    public static string ToApiKey(this UserInfo user)
    {
        return $"user{OpenSubsonicServer.ApiIdSeparator}{user.ApiKey}";
    }
}
