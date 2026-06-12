using QuizzArena.Quizzing.Application.DTOs.Question;

namespace QuizzArena.Quizzing.Application.DTOs.Quiz;

public class CreateQuizDto : BaseQuizDto
{
    public IEnumerable<CreateQuestionDto> Questions { get; set; } = [];
}
