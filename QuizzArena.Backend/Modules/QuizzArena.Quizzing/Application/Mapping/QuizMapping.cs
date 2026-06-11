using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Mapping;

internal class QuizMapping : Profile
{
    public QuizMapping()
    {
        CreateMap<Quiz, QuizDto>().ReverseMap();
        CreateMap<Quiz, CreateQuizDto>().ReverseMap();
    }
}
