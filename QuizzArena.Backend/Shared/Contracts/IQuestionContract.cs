using Shared.Contracts.DTOs;

namespace Shared.Contracts;

public interface IQuestionContract
{
    public Task<List<Guid>> CreateQuestions(List<QuestionCreationRequestDTO> questionCreationRequestDTOs);
}
