using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Option;

public record StartAttemptQuestionResponseDto
{
    public Guid Id { get; set; }
    public string Statement { get; set; } = "";
    public QuestionType QuestionType { get; set; }
    public List<StartAttemptOptionResponseDto> Options { get; set; } = [];
}
