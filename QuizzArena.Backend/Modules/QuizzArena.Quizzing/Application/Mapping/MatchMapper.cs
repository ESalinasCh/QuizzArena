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
        CreateMap<Match, MatchDetailResponseDto>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.TimeMinutes))
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.QuestionsAmount));
    }

}
