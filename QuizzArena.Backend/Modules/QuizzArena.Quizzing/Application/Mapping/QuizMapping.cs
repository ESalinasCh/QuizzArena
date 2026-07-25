using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.DTOs.QuizQuestion;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal sealed class QuizMapping : Profile
{
    public QuizMapping()
    {
        CreateMap<Quiz, QuizDto>().ReverseMap();
        CreateMap<Quiz, CreateQuizDto>().ReverseMap();

        CreateMap<CreateExamDto, Quiz>()
            .ForMember(dest => dest.Origin, opt => opt.Ignore());

        CreateMap<Quiz, CreateQuizResponseDto>()
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.QuizQuestions));
        CreateMap<QuizQuestion, QuizQuestionResponseDto>();

        CreateMap<Quiz, TeacherQuizResponseDto>()
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.QuizQuestions));
        CreateMap<QuizQuestion, TeacherQuizQuestionResponseDto>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Question.Content))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Question.Type));
    }
}
