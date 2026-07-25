using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetTeacherQuizzesUseCase
{
    Task<List<TeacherQuizResponseDto>> Execute(QuizOrigin? origin);
}
