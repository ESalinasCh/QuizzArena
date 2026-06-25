namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record MatchAttemptSmallProgressDto
{
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }
}
