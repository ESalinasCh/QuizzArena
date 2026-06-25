namespace QuizzArena.Quizzing.Application.DTOs.Quiz;

public class CreateExamDto : BaseQuizDto
{
    public IEnumerable<Guid> QuestionIds { get; set; } = [];
}
