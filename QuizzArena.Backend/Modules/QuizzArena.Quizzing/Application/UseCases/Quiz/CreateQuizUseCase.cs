
using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Quiz;

namespace QuizzArena.Quizzing.Application.UseCases.Quiz;

internal class CreateQuizUseCase(
    IQuizRepository repository,
    IMapper mapper,
    CreateQuizDtoValidator createValidator,
    CreateQuestionsUseCase createQuestionsUseCase
) : ICreateQuizUseCase
{
    public async Task<QuizDto> Execute(CreateQuizDto dto, Guid classSourceId)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        Domain.Entities.Quiz quiz = mapper.Map<Domain.Entities.Quiz>(dto);
        await repository.CreateAsync(quiz);
        await createQuestionsUseCase.Execute(dto.QuizQuestions, quiz.Id);
        return mapper.Map<QuizDto>(quiz);
    }
}
