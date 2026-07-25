using QuizzArena.Quizzing.Application.DTOs.QuizQuestion;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Quiz;

public class TeacherQuizResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuizStatus Status { get; set; } = QuizStatus.draft;
    public QuizOrigin Origin { get; set; }
    public IEnumerable<TeacherQuizQuestionResponseDto> Questions { get; set; } = [];
}
