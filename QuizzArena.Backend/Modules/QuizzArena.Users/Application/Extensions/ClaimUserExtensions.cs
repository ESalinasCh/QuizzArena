using System.Security.Claims;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Application.Extensions;

public static class ClaimUserExtensions
{
    public static CreateUserDto ToCreateUserDto(
        this ClaimsPrincipal user)
    {
        return new CreateUserDto
        {
            Id = Guid.Parse(user.GetClaim(ClaimTypes.Sub)),
            ProviderId = user.GetClaim(ClaimTypes.Sub),
            UserName = user.GetClaim(ClaimTypes.PreferredUsername),
            FirstName = user.GetClaim(ClaimTypes.GivenName),
            LastName = user.GetClaim(ClaimTypes.FamilyName),
            Email = user.GetClaim(ClaimTypes.Email),
            ExternalProvider = user.GetClaim(ClaimTypes.ExternalProvider),
            Role = user.GetClaimRole()
        };
    }
}
