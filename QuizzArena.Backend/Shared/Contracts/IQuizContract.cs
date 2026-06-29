using Shared.Contracts.DTOs;

namespace Shared.Contracts;

public interface IQuizContract
{
    public Task<Guid> CreateQuiz(QuizCreationRequestDTO quizRequestDto);
} 
