using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class GetMatchAttemptsByStudent(
    ICurrentUser currentUser,
    IMatchQueriesRepository matchRepository,
    ICourseContract courseContract,
    MatchAttemptFiltersValidator filtersValidator
    ) : IGetMatchAttemptsByStudent
{
    public async Task<List<GetMatchAttemptDTO>> Execute(MatchAttemptFilters filters)
    {
        Guid studentId = Guid.Parse(currentUser.UserId);
        await filtersValidator.ValidateAndThrowAsync(filters);

        List<MatchAttempt> matchAttempts = await matchRepository.GetAttemptsByStudentId(studentId, filters);
        var matches = await matchRepository.GetMatchesByIds(matchAttempts.Select(x => x.MatchId).Distinct().ToList());
        var courses = await courseContract.GetCoursesByIds(matches.Select(x => x.CourseId).Distinct().ToList());

        var matchesDictionary = matches.ToDictionary(x => x.Id);
        var coursesDictionary = courses.ToDictionary(x => x.Id);

        return matchAttempts.Select(x =>
        {
            var match = matchesDictionary[x.MatchId];
            var course = coursesDictionary[match.CourseId];
            return new GetMatchAttemptDTO()
            {
                Id = x.Id,
                Title = match.Title,
                CourseName = course.CourseName,
                StartedAt = x.StartDateTime,
                CompletedAt = x.EndDateTime,
                Score = x.Score,
                Status = x.Status,
                Duration = match.TimeMinutes
            };

        }).ToList();
    }

}
