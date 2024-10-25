using System.Security.Claims;
using Melodee.Entities;

namespace Melodee.Services;

public interface IAuthDataService
{
    ServiceResponse<ClaimsPrincipal> Login(string email, string password);
}
