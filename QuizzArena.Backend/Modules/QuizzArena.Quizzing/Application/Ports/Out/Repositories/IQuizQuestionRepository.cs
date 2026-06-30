using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out.Repositories;

public interface IQuizQuestionRepository
{
    public Task CreateMultipleAsync(IEnumerable<QuizQuestion> questions);
    public Task<List<Question>> GetQuestionsByQuizIdAsync(Guid QuizId);
    public Task<List<AugmentedQuestionDto>> GetQuestionsAndScoreByQuizIdAsync(Guid QuizId);
}
