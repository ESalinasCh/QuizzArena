namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQuestionQueriesRepository
{
    Task<Dictionary<Guid, int>> GetQuestionCountsByQuizIdsAsync(List<Guid> quizIds);
}
