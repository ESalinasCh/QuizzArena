using System.Security.Claims;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetClaim(this ClaimsPrincipal user, string claimType)
    {
        return user.FindFirst(claimType)?.Value ?? "";
    }

    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.GetClaim(ClaimTypes.Sub);
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

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.GetClaimRole().ToString();
    }
}
