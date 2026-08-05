using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In.Match;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class GetMatchByIdUseCase(
    IMatchRepository matchRepository,
    ICourseContract courseImpl,
    ICurrentUser currentUser,
    IMapper mapper
    ) : IGetMatchByIdUseCase
{
    public async Task<MatchDetailResponseDto> Execute(Guid matchId)
    {
        Match match = await matchRepository.GetMatchByIdAsync(matchId)
            ?? throw new KeyNotFoundException("Match not found.");

        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByTeacherId(Guid.Parse(currentUser.UserId));

        if (!courses.Any(c => c.Id == match.CourseId))
        {
            throw new UnauthorizedAccessException("User doesn't belong to this match's course.");
        }

        return mapper.Map<MatchDetailResponseDto>(match);
    }
}
