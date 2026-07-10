using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal sealed class MatchAttemptMapper : Profile
{
    public MatchAttemptMapper()
    {
        CreateMap<MatchAttempt, MatchAttemptGradesResponseDto>().ReverseMap();
        CreateMap<MatchAttempt, OtherAttemptsGradesResponseDto>().ReverseMap();
    }
}
