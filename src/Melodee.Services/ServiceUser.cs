using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Services;

/// <summary>
///     This user is used by services for calls without a user.
/// </summary>
public sealed class ServiceUser : User
{
    public const int ServiceUserId = 99;

    public static readonly Lazy<ServiceUser> Instance = new(NewServiceUser);

    public ServiceUser()
    {
        Id = ServiceUserId;
    }

    private static ServiceUser NewServiceUser()
    {
        return new ServiceUser
        {
            CreatedAt = default,
            PublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64(),
            UserName = "ServiceUser",
            UserNameNormalized = "ServiceUser".ToUpperInvariant(),
            Email = "serviceuser@local.lan",
            EmailNormalized = "serviceuser@local.lan".ToNormalizedString()!,
            IsAdmin = true,
            PasswordEncrypted = string.Empty
        };
    }
}
