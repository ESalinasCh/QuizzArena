using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;

namespace QuizzArena.Quizzing.Application.UseCases.Question
{
    public class CreateQuestionUseCase(IQuestionRepository repository) : ICreateQuestionUseCase
    {
        public async Task<CreateQuestionResponseDto> Execute(CreateQuestionRequestDto dto)
        {
            return new CreateQuestionResponseDto();
        }

    }
}
