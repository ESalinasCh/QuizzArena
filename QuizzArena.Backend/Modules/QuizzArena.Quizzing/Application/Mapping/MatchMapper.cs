using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal sealed class MatchMapper : Profile
{
    public MatchMapper()
    {
        CreateMap<Match, MatchCreateDto>().ReverseMap();
        CreateMap<MatchCreatedResponseDto, Match>().ReverseMap();
    }

}
