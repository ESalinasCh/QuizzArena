using AutoMapper;
using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;

public class GetMatchAttemptGradesUseCase(
    IMatchAttemptRepository matchAttemptRepository,
    MatchAttemptFiltersValidator filtersValidator,
    IMapper mapper
) : IGetMatchAttemptGradesUseCase
{
    public async Task<List<MatchAttemptGradesResponseDto>> Execute(
        Guid matchId,
        MatchAttemptFilters filters
    )
    {
        await filtersValidator.ValidateAndThrowAsync(filters);
        List<MatchAttempt> bestAttempts = await matchAttemptRepository.GetAttemptsByMatchId(matchId, filters);
        if (bestAttempts.Count > 0)
        {
            List<Guid> userIds = bestAttempts.Select(x => x.UserId).ToList();
            List<MatchAttempt> allAttempts = await matchAttemptRepository.GetAttemptsByUserIds(matchId, userIds);
            foreach (MatchAttempt best in bestAttempts)
            {
                best.OtherAttempts = allAttempts
                    .Where(x =>
                        x.UserId == best.UserId &&
                        x.Id != best.Id)
                    .OrderByDescending(x => x.Score)
                    .ToList();
            }
        }

        return bestAttempts.Select(q => mapper.Map<MatchAttemptGradesResponseDto>(q)).ToList();
    }
}
