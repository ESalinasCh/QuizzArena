using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;

namespace QuizzArena.Users.Application.UseCases.User;

public class LogInUserUseCase(IUserRepository repository) : ILogInUserUseCase
{
    public async Task<LogInResponseDto> Execute(LogInRequestDto dto)
    {
        return new LogInResponseDto();
    }
}
