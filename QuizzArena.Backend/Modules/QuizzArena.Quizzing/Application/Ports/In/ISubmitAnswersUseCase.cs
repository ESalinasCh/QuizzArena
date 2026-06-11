
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ISubmitAnswersUseCase
{
    public Task<SubmitAnswersResponseDto> Execute(SubmitAnswersRequestDto dto);
}
