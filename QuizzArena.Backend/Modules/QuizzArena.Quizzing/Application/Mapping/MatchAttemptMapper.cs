using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Mapping;

internal sealed class MatchAttemptMapper : Profile
{
    public MatchAttemptMapper()
    {
        CreateMap<MatchAttempt, MatchAttemptGradesResponseDto>().ReverseMap();
        CreateMap<MatchAttempt, OtherAttemptsGradesResponseDto>().ReverseMap();

        CreateMap<MatchAttempt, StartAttemptResponseDto>()
            .ForMember(d => d.MatchAttemptId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Questions, o => o.Ignore())
            .ForMember(d => d.AnsweredQuestions, o => o.MapFrom(s => s.Answers.Count))
            .ForMember(d => d.TotalQuestions, o => o.MapFrom(s => s.MatchAttemptQuestions.Count));

        CreateMap<AugmentedQuestionDto, StartAttemptQuestionResponseDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Question.Id))
            .ForMember(d => d.Statement, o => o.MapFrom(s => s.Question.Content))
            .ForMember(d => d.QuestionType, o => o.MapFrom(s => s.Question.Type))
            .ForMember(d => d.Options, o => o.MapFrom(s => s.Question.Options));

        CreateMap<MatchAttemptQuestion, StartAttemptQuestionResponseDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Question != null ? s.Question.Id : Guid.Empty))
            .ForMember(d => d.Statement, o => o.MapFrom(s => s.Question != null ? s.Question.Content : string.Empty))
            .ForMember(d => d.QuestionType, o => o.MapFrom(s => s.Question != null ? s.Question.Type : QuestionType.SingleChoice))
            .ForMember(d => d.Options, o => o.MapFrom(s => s.Question != null ? s.Question.Options : new List<Option>()));

        CreateMap<Option, StartAttemptOptionResponseDto>()
            .ForMember(d => d.Label, o => o.MapFrom(s => s.Description));
    }
}
