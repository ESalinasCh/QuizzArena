using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal sealed class QuestionMapping : Profile
{
    public QuestionMapping()
    {
        CreateMap<Question, QuestionDto>().ReverseMap();
        CreateMap<Question, CreateQuestionDto>().ReverseMap()
            .ForMember(
            dest => dest.Options,
            opt => opt.Ignore());
    }
}
