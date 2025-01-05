using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class UserExtensions
{
    public static string ToAvatarFileName(this User user, string libraryPath)
    {
        return Path.Combine(libraryPath, $"{user.Id.ToStringPadLeft(8)}.gif");
    }
    
    public static Common.Models.OpenSubsonic.User ToApiUser(this User user)
    {
        return new Common.Models.OpenSubsonic.User(user.UserName, user.IsAdmin, user.Email, user.HasStreamRole, user.IsScrobblingEnabled, user.HasDownloadRole, user.HasShareRole, user.HasJukeboxRole, user.LastUpdatedAt?.ToString() ?? user.CreatedAt.ToString());
    }

    public static UserInfo ToUserInfo(this User user)
    {
        var roles = new List<string>();

        if (user.HasSettingsRole)
        {
            roles.Add(nameof(User.HasSettingsRole));
        }

        if (user.HasDownloadRole)
        {
            roles.Add(nameof(User.HasDownloadRole));
        }

        if (user.HasUploadRole)
        {
            roles.Add(nameof(User.HasUploadRole));
        }

        if (user.HasPlaylistRole)
        {
            roles.Add(nameof(User.HasPlaylistRole));
        }

        if (user.HasCoverArtRole)
        {
            roles.Add(nameof(User.HasCoverArtRole));
        }

        if (user.HasCommentRole)
        {
            roles.Add(nameof(User.HasCommentRole));
        }

        if (user.HasPodcastRole)
        {
            roles.Add(nameof(User.HasPodcastRole));
        }

        if (user.HasStreamRole)
        {
            roles.Add(nameof(User.HasStreamRole));
        }

        if (user.HasJukeboxRole)
        {
            roles.Add(nameof(User.HasJukeboxRole));
        }

        if (user.HasShareRole)
        {
            roles.Add(nameof(User.HasShareRole));
        }

        if (user.IsAdmin)
        {
            roles.Add(RoleNameRegistry.Administrator);
        }

        return new UserInfo(user.Id, user.ApiKey, user.UserName, user.Email, user.PublicKey, user.PasswordEncrypted)
        {
            Roles = roles
        };
    }

    public static string Encrypt(this User user, string plainText, IMelodeeConfiguration configuration)
    {
        return EncryptionHelper.Encrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, plainText, user.PublicKey);
    }

    public static string Decrypt(this User user, string encryptedText, IMelodeeConfiguration configuration)
    {
        return EncryptionHelper.Decrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, encryptedText, user.PublicKey);
    }
}
