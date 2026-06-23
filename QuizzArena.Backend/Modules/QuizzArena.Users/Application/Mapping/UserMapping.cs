using AutoMapper;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Mapping;

internal sealed class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, CreateUserDto>().ReverseMap();
    }
}
