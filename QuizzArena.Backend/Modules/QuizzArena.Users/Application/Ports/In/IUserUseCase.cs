using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.In;

public interface IUserUseCase
{
    public Task<bool> ExistsAsync(string providerId);
    public Task<UserBaseDto> Register(CreateUserDto user);
}
