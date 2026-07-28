using QuizzArena.Quizzing.Application.DTOs.Quiz;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface IGetTeacherQuizzesUseCase
{
    Task<List<TeacherQuizResponseDto>> Execute(QuizQueryParametersDto query);
}
