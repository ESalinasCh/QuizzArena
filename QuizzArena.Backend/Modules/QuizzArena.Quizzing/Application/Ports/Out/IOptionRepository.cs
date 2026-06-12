
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.Ports.Out;

public interface IOptionRepository
{
    Task<List<Option>> GetByIdsAsync(List<Guid> optionIds);
}