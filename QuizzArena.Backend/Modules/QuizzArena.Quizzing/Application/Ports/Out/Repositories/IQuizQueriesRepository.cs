using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQueriesRepository
{
    public Task<List<Quiz>> GetByTeacherIdAsync(Guid teacherId, QuizOrigin? origin);
}
