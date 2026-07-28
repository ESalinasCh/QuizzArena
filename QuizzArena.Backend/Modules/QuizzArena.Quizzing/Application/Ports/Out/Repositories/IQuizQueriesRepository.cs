using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQueriesRepository
{
    public Task<List<Quiz>> GetByTeacherIdAsync(Guid teacherId, QuizQueryParametersDto query);
}
