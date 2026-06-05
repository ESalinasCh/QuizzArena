using System.Security.Claims;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Application.Extensions;

public static class ClaimUserExtensions
{
    public static string GetClaim(this ClaimsPrincipal user, string claimType)
    {
        return user.FindFirst(claimType)?.Value ?? "";
    }

    public static UserRole GetClaimRole(this ClaimsPrincipal user)
    {
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            string roleName = role.ToString().ToLowerInvariant();
            if (user.IsInRole(roleName))
            {
                return role;
            }
        }
        throw new InvalidOperationException("No valid role found");
    }

    public static CreateUserDto ToCreateUserDto(
        this ClaimsPrincipal user)
    {
        return new CreateUserDto
        {
            ProviderId = user.GetClaim("sub"),
            UserName = user.GetClaim("preferred_username"),
            FirstName = user.GetClaim("given_name"),
            LastName = user.GetClaim("family_name"),
            Email = user.GetClaim("email"),
            ExternalProvider = user.GetClaim("external_provider"),
            Role = user.GetClaimRole()
        };
    }
}
