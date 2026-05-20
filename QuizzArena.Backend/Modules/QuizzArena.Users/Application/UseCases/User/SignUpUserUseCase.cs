using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;

namespace QuizzArena.Users.Application.UseCases.User
{
    public class SignUpUserUseCase(IUserRepository repository) : ISignUpUserUseCase
    {
        public async Task<SignUpResponseDto> Execute(SignUpRequestDto dto)
        {
            return new SignUpResponseDto();
        }
    }
}
