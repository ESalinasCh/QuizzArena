using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.DTOs.Option;

public record StartAttemptQuestionResponseDto
{
    public Guid Id { get; set; }
    public string Statement { get; set; } = "";
    public List<StartAttemptOptionResponseDto> Options { get; set; } = [];
}
