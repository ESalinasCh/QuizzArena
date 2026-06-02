using QuizzArena.Users.Application.DTOs.User;

namespace QuizzArena.Users.Application.Ports.In;

public interface IUserUseCase
{
    public Task<bool> ExistsAsync(string providerId);
    public Task<UserBaseDto> Register(CreateUserDto user);
}
