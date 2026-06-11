namespace QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

public class SubmitAnswersResponseDto
{
	public Guid AttemptId { get; set; }
	public int ScorePercentage { get; set; }
	public int CorrectCount { get; set; }
	public int IncorrectCount { get; set; }
	public int TotalQuestions { get; set; }
	public required List<QuestionResultDto> Questions { get; set; }
}

public record QuestionResultDto
(
	Guid Id,
	int Number,
	string Text,
	Guid SelectedOptionId,
	Guid CorrectOptionId,
	bool IsCorrect
);
