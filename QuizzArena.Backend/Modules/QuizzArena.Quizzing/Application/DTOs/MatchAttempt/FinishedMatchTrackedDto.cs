namespace QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

public record FinishedMatchTrackedDto
{
    public Guid AttemptId { get; set; }
    public int AnsweredQuestions { get; set; }
    public int TotalQuestions { get; set; }

    public List<AnswerTrackedDto> Answers { get; set; } = [];
}
public record AnswerTrackedDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Text { get; set; } = "";
    public Guid? SelectedOptionId { get; set; }
}
