using Melodee.Common.Data.Models;

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
            UserName = "ServiceUser",
            UserNameNormalized = "ServiceUser".ToUpperInvariant(),
            Email = "serviceuser@local.lan",
            EmailNormalized = "serviceuser@local.lan".ToUpperInvariant(),
            IsAdmin = true,
            PasswordHash = string.Empty
        };
    }
}
