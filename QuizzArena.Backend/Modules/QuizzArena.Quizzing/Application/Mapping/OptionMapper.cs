using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal class OptionMapper : Profile
{
    public OptionMapper()
    {
        CreateMap<Option, OptionDto>().ReverseMap();
        CreateMap<Option, CreateOptionDto>().ReverseMap();
    }
}
