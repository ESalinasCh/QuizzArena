using QuizzArena.Users.Application.DTOs.User;

namespace QuizzArena.Users.Application.Ports.In;

public interface ISignUpUserUseCase
{
    Task<SignUpResponseDto> Execute(SignUpRequestDto dto);
}
