using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public class MatchAttemptGradesResponseDto
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
    public decimal Score { get; set; }
    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }
    public List<OtherAttemptsGradesResponseDto> OtherAttempts { get; set; } = [];
}

public class OtherAttemptsGradesResponseDto
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
    public decimal Score { get; set; }
}
