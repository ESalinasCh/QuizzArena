using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.QuestionUseCases;

public class UpdateQuestionUseCase(
    IQuestionRepository questionRepository,
    IMapper mapper,
    UpdateQuestionDtoValidator validator
) : IUpdateQuestionUseCase
{
    public async Task<ResponseQuestionDto> Execute(UpdateQuestionDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        Question question = await questionRepository.GetByIdWithOptionsAsync(dto.QuestionId)
            ?? throw new InvalidOperationException("Question doesn't exist");

        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (dto.Content is not null)
        {
            question.Content = dto.Content;
        }

        if (dto.Justification is not null)
        {
            question.Justification = dto.Justification;
        }

        if (dto.Status.HasValue)
        {
            question.Status = dto.Status.Value;
        }

        if (dto.Type.HasValue)
        {
            question.Type = dto.Type.Value;
        }

        question.UpdatedAt = now;

        if (dto.Options.Count > 0)
        {
            ApplyOptionUpdates(question, dto.Options, now);
        }

        await questionRepository.UpdateAsync(question);

        return mapper.Map<ResponseQuestionDto>(question);
    }

    private static void ApplyOptionUpdates(Question question, List<UpdateOptionDto> updates, DateTimeOffset now)
    {
        foreach (UpdateOptionDto update in updates)
        {
            Option option = question.Options.FirstOrDefault(o => o.Id == update.OptionId && !o.Deleted)
                ?? throw new InvalidOperationException($"Option '{update.OptionId}' doesn't exist for this question");

            if (update.Description is not null)
            {
                option.Description = update.Description;
            }

            if (update.IsCorrect.HasValue)
            {
                option.IsCorrect = update.IsCorrect.Value;
            }

            if (update.Position.HasValue)
            {
                option.Position = update.Position.Value;
            }

            option.UpdatedAt = now;
        }
    }
}
