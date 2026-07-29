using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record StartAttemptResponseDto
{
    public Guid MatchId { get; set; }
    public Guid MatchAttemptId { get; set; }
    public List<StartAttemptQuestionResponseDto> Questions { get; set; } = [];
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }
}
