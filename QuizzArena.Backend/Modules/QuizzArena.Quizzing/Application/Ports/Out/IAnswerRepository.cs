using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IAnswerRepository
{
    Task<Answer> SetAnswer(Guid id);
}
