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
    IQuizRepository quizRepository
    ) : ICreateMatchUseCase
{
    public async Task<MatchCreatedResponseDto> Execute(MatchCreateDto dto)
    {
        await createValidator.ValidateAndThrowAsync(dto);
        Quiz? quiz = await quizRepository.GetByIdAsync(dto.QuizId) ?? throw new KeyNotFoundException("Quiz not found.");
        Match match = mapper.Map<Match>(dto);

        match.Mode = MatchMode.Exam;
        match.CreatedAt = DateTimeOffset.UtcNow;
        match.UpdatedAt = DateTimeOffset.UtcNow;
        match.Title = quiz.Title + " " + match.CreatedAt;
        match.QuestionsAmount = null;
        match.Code = Random.Shared.Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
        match.QuestionsAmount = null;

        Match createdMatch = await matchRepository.CreateMatchAsync(match);

        MatchCreatedResponseDto response = mapper.Map<MatchCreatedResponseDto>(createdMatch);

        return response;
    }
}
