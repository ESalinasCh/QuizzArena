using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchQueryParametersDto
{
    public string? Code { get; set; }
    public MatchStatus? Status { get; set; }
    public MatchMode? Mode { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? QuizId { get; set; }
}
