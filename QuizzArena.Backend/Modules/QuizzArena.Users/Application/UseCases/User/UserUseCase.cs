using AutoMapper;
using FluentValidation;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.Validators;

namespace QuizzArena.Users.Application.UseCases.User;

internal class UserUseCase(
    IUserRepository repository,
    IMapper mapper,
    UserCreateDtoValidator createValidator
    ) : IUserUseCase
{
    public async Task<bool> ExistsAsync(string providerId)
    {
        return await repository.ExistsAsync(providerId);
    }

    public async Task<UserDto> Register(CreateUserDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        Domain.Entities.User user = mapper.Map<Domain.Entities.User>(dto);
        await repository.Register(user);
        return mapper.Map<UserDto>(user);
    }
}
