using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using Shared.Contracts;
using Shared.Contracts.DTOs;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class GetMatchesUseCase(
    IValidator<MatchQueryParametersDto> queryValidator,
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
                Title = m.Title,
                CourseName = course.CourseName
            };
        }).ToList();

        return matchesDtos;
    }
}
