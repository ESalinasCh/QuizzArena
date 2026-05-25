using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;

namespace QuizzArena.Quizzing.Application.UseCases.Question;

public class DeleteQuestionUseCase(IQuestionRepository repository) : IDeleteQuestionUseCase
{
    public async Task<DeleteQuestionResponseDto> Execute(DeleteQuestionRequestDto dto)
    {
        return new DeleteQuestionResponseDto();
    }
}
