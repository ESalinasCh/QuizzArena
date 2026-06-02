using Microsoft.AspNetCore.Authorization;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Extensions;
using QuizzArena.Users.Application.Ports.In;

namespace Host.Security;

public class UserValidationMiddleware
{
    private readonly RequestDelegate _next;

    public UserValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserUseCase userUseCase)
    {
        Endpoint? endpoint = context.GetEndpoint();
        bool hasAuthorize = endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null;
        if (!hasAuthorize)
        {
            await _next(context);
            return;
        }

        AuthorizeAttribute? authorize = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
        if (authorize is not null && context.User.Identity?.IsAuthenticated == true)
        {
            string? sub = context.User.FindFirst("sub")?.Value;
            if (sub is null)
            {
                context.Response.StatusCode = 401;
                return;
            }
            bool exists = await userUseCase.ExistsAsync(sub);
            if (!exists)
            {
                CreateUserDto userToCreate = context.User.ToCreateUserDto();
                UserBaseDto newUser = await userUseCase.Register(userToCreate);
                Console.WriteLine(newUser);
            }
        }

        await _next(context);
    }
}
