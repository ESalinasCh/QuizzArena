using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.ExternalServices;

public class MatchContractImpl(
    IMatchRepository matchRepository
) : IMatchContract
{
    public async Task<Guid> CreateAutomaticMatch(MatchCreationAutomaticRequestDTO matchAutomaticCreationDto)
    {
        Match match = await matchRepository.CreateMatchAsync(new Match()
        {
            Id = Guid.NewGuid(),
            Code = Guid.NewGuid().ToString()[..8].ToUpperInvariant(),
            Title = matchAutomaticCreationDto.Title,
            Status = MatchStatus.Active,
            StartedAt = DateTimeOffset.UtcNow,
            FinishedAt = null,
            Mode = MatchMode.Solo,
            TimeMinutes = matchAutomaticCreationDto.QuestionAmount,
            QuestionsAmount = null,
            AttemptsAmount = 1,
            ShuffleQuestion = true,
            ShuffleOptions = true,
            Deleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletedAt = null,
            CourseId = matchAutomaticCreationDto.CourseId,
            QuizId = matchAutomaticCreationDto.QuizId,
        });

        return match.Id;
    }
}
