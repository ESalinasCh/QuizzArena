using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

public class SubmitAnswersMappingProfile : Profile
{
    public SubmitAnswersMappingProfile()
    {
        CreateMap<SubmitAnswerBody, Answer>()
            .ForMember(
                dest => dest.Id,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.OptionId,
                opt => opt.MapFrom(src => src.SelectedOptionId)
            )

            .ForMember(
                dest => dest.QuestionId,
                opt => opt.MapFrom(src => src.QuestionId)
            )

            .ForMember(
                dest => dest.AnsweredAt,
                opt => opt.MapFrom(src => src.AnsweredAt)
            )

            .ForMember(
                dest => dest.IsCorrect,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.TimeMs,
                opt => opt.Ignore()
            )

            .ForMember(
                dest => dest.MatchAttemptId,
                opt => opt.Ignore()
            );
    }
}
