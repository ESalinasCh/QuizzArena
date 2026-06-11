using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

internal interface IOptionRepository
{
    public Task CreateMultipleAsync(IEnumerable<Option> options);
}
