using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class UserExtensions
{
    public static Common.Models.OpenSubsonic.User ToApiUser(this User user) 
        => new(user.UserName, user.IsAdmin, user.Email, user.HasStreamRole, user.IsScrobblingEnabled, user.HasDownloadRole, user.HasShareRole, user.HasJukeboxRole);

    public static UserInfo ToUserInfo(this User user) => new(user.Id, user.ApiKey, user.UserName, user.Email);
    
    public static string Encrypt(this User user, string plainText, IMelodeeConfiguration configuration) 
        => EncryptionHelper.Encrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, plainText, user.PublicKey);
    
    public static string Decrypt(this User user, string encryptedText, IMelodeeConfiguration configuration)
        => EncryptionHelper.Decrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, encryptedText, user.PublicKey);
}
