using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class GetTeacherQuizzesUseCase(

    ) : IGetTeacherQuizzesUseCase
{
    // Use repo to get the quizzes existing for the current teacher
    // I need teacher id (current user)
    // I need to get his quizzes (inlcuding the questions). So, I need the repository or whatever port out does this
    // I need to return them
    public Task<List<QuizDto>> Execute() => throw new NotImplementedException();
}
