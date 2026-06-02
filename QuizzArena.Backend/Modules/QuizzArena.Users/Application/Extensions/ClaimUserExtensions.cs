using System.Security.Claims;
using System.Text.Json;
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
        string? realmAccess = user.GetClaim("realm_access");

        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            throw new InvalidOperationException("realm_access claim not found");
        }
        JsonDocument json = JsonDocument.Parse(realmAccess);
        JsonElement.ArrayEnumerator roles = json.RootElement.GetProperty("roles").EnumerateArray();
        foreach (JsonElement roleElement in roles)
        {
            string? role = roleElement.GetString();
            if (Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
            {
                return parsedRole;
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
