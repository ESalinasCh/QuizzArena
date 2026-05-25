using QuizzArena.Quizzing.Application.DTOs.Answer;
using QuizzArena.Quizzing.Application.Ports.In.Answer;
using QuizzArena.Quizzing.Application.Ports.Out;

namespace QuizzArena.Quizzing.Application.UseCases.Answer;

public class SetAnswerUseCase(IAnswerRepository repository) : ISetAnswerUseCase
{
    public async Task<SetAnswerResponseDto> Execute(SetAnswerRequestDto dto)
    {
        return new SetAnswerResponseDto();
    }
}
