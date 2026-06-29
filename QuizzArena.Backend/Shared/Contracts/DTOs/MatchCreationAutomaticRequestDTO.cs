namespace Shared.Contracts.DTOs;

public record MatchCreationAutomaticRequestDTO
{
    public string Title { get; set; } = "Unnamed";
    public int QuestionAmount { get; set; }
    public Guid CourseId { get; set; }
    public Guid QuizId { get; set; }
}
