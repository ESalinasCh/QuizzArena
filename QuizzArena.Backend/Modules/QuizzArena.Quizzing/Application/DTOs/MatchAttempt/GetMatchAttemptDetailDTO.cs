using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public class GetMatchAttemptDetailDTO
{
    public Guid Id { get; set; }
    public decimal? Score { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public IEnumerable<GetMatchAttemptQuestionDTO> Questions { get; set; } = [];

}

public class GetMatchAttemptQuestionDTO
{
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = "";
    public string? Justification { get; set; } = "";
    public Guid? SelectedOptionId { get; set; }
    public bool? IsCorrect { get; set; }
    public IEnumerable<GetMatchAttemptOptionDTO> Options { get; set; } = [];
}

public class GetMatchAttemptOptionDTO
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "";
    public bool? IsCorrect { get; set; }
}
