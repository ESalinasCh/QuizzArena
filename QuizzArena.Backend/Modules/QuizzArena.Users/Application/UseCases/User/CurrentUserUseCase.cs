using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QuizzArena.Users.Application.Extensions;
using Shared.Contracts;

namespace QuizzArena.Users.Application.UseCases.User;

internal class CurrentUserUseCase(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new UnauthorizedAccessException();
    public string UserId => User.GetUserId();
    public string Role => User.GetRole();
}
