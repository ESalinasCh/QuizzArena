namespace QuizzArena.Quizzing.Application.DTOs.QuizQuestion;

public class BaseQuizQuestionDto
{
    public int Position { get; set; }
    public int ValueScore { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
}
