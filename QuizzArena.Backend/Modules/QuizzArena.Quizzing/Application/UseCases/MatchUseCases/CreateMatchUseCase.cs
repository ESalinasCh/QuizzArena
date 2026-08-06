using System.Globalization;
using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.Match;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class CreateMatchUseCase(
    IMatchRepository matchRepository,
    CreateMatchDtoValidator createValidator,
    IMapper mapper,
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository
    ) : ICreateMatchUseCase
{
    public async Task<MatchCreatedResponseDto> Execute(MatchCreateDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);

        Quiz? quiz = await quizRepository.GetByIdAsync(dto.QuizId) ?? throw new KeyNotFoundException("Quiz not found.");
        Match match = mapper.Map<Match>(dto);

        var quizQuestions = await quizQuestionRepository.GetQuestionsByQuizIdAsync(dto.QuizId);

        if (quizQuestions.Count < dto.QuestionsAmount)
        {
            throw new InvalidOperationException($"Question Amount can not be greater than the total number of questions available for this match ({quizQuestions.Count})");
        }

        match.Mode = MatchMode.Exam;
        match.CreatedAt = DateTimeOffset.UtcNow;
        match.UpdatedAt = DateTimeOffset.UtcNow;
        match.Title = dto.Title;
        match.QuestionsAmount = dto.QuestionsAmount;
        match.Code = Random.Shared.Next(100000, 999999).ToString(CultureInfo.InvariantCulture);

        Match createdMatch = await matchRepository.CreateMatchAsync(match);

        MatchCreatedResponseDto response = mapper.Map<MatchCreatedResponseDto>(createdMatch);

        return response;
    }
}
