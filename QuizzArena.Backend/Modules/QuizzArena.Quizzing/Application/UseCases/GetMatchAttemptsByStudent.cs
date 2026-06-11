using System.Text.RegularExpressions;
using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;

namespace QuizzArena.Quizzing.Application.UseCases;

internal class GetMatchAttemptsByStudent(IMatchQueriesRepository matchRepository, ICourseContract courseContract)
{
    public async Task<List<GetMatchAttemptDTO>> Execute(Guid studentId)
    {
        List<MatchAttempt> matchAttempts = await matchRepository.GetAttemptsByStudentId(studentId);
        var matches = await matchRepository.GetMatchesByIds(matchAttempts.Select(x => x.MatchId).Distinct().ToList());
        var courses = await courseContract.GetCoursesByIds(matches.Select(x=> x.CourseId).Distinct().ToList());

        var matchesDictionary = matches.ToDictionary(x => x.Id);
        var coursesDictionary = courses.ToDictionary(x => x.Id);

        return matchAttempts.Select(x =>
        {
            var match = matchesDictionary[x.MatchId];
            var course = coursesDictionary[match.CourseId];
            return new GetMatchAttemptDTO()
            {
                Id = x.Id,
                Title = "", //missing
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
