using QuizzArena.Users.Application.DTOs.User;

namespace QuizzArena.Users.Application.Ports.In;

public interface ILogInUserUseCase
{
    Task<LogInResponseDto> Execute(LogInRequestDto dto);
}
