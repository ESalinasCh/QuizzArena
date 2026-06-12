
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Validators;

namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class SubmitAnswersUseCase(
    SubmitAnswersRequestValidator submitAnswersValidator
) : ISubmitAnswersUseCase
{
    public async Task<SubmitAnswersResponseDto> Execute(SubmitAnswersRequestDto dto)
    {
        await submitAnswersValidator.ValidateAndThrowAsync(dto);

        // TODO
        throw new NotImplementedException();
    }
}
