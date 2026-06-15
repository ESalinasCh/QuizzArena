using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Validators;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.UseCases;

public class GetMatchesUseCase(
    MatchQueryParametersValidator queryValidator,
    ICourseContract courseImpl,
    IMatchRepository matchRepository,
    ICurrentUser currentUser
) : IGetMatchesUseCase
{
    public async Task<List<MatchResponseDto>> Execute(MatchQueryParametersDto query)
    {
        await queryValidator.ValidateAndThrowAsync(query);

        string userId = currentUser.UserId;
        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByStudent(Guid.Parse(userId));
        List<Match> matches = await matchRepository.GetMatchesAsync(courses.Select(c => c.Id).ToList(), query);

        List<MatchResponseDto> matchesDtos = matches.Select(m =>
        {
            CourseSummaryDTO course = courses.First(c => c.Id == m.CourseId);
            return new MatchResponseDto()
            {
                Id = m.Id,
                Title = "aaaa", // Replace
                CourseName = course.CourseName
            };
        }).ToList();

        return matchesDtos;
    }
}
