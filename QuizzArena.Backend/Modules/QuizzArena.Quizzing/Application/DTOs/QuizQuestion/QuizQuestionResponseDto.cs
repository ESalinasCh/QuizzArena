namespace QuizzArena.Quizzing.Application.DTOs.QuizQuestion;

public class QuizQuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public int Position { get; set; }
    public int ValueScore { get; set; }
}
