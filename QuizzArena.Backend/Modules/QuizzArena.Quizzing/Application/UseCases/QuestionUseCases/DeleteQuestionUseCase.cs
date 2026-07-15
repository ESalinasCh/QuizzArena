using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;

public class DeleteQuestionUseCase(
    IQuestionRepository questionRepository,
    IMapper mapper
) : IDeleteQuestionUseCase
{
    public async Task<ResponseQuestionDto> Execute(Guid questionId)
    {
        Question question = await questionRepository.GetByIdWithOptionsAsync(questionId)
            ?? throw new InvalidOperationException("Question doesn't exist");

        DateTimeOffset now = DateTimeOffset.UtcNow;

        question.Deleted = true;
        question.DeletedAt = now;
        question.UpdatedAt = now;

        foreach (Option option in question.Options.Where(o => !o.Deleted))
        {
            option.Deleted = true;
            option.DeletedAt = now;
            option.UpdatedAt = now;
        }

        await questionRepository.UpdateAsync(question);

        return mapper.Map<ResponseQuestionDto>(question);
    }
}
