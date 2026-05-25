using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.Out;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

public class UpdateQuizUseCase(IQuizRepository repository) : IUpdateQuizUseCase
{
    public async Task<UpdateQuizResponseDto> Execute(UpdateQuizRequestDto dto)
    {
        return new UpdateQuizResponseDto();
    }
}
