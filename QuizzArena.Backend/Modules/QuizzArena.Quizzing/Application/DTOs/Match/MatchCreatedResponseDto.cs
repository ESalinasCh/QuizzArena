using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public class MatchCreatedResponseDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid CourseId { get; set; }
    public MatchStatus Status { get; set; }
}
