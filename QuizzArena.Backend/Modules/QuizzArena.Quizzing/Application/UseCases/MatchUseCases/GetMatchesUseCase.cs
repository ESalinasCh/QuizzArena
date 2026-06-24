using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class GetMatchesUseCase(
    IValidator<MatchQueryParametersDto> queryValidator,
    ICourseContract courseImpl,
    IMatchRepository matchRepository,
    IQuizQuestionQueriesRepository quizQuestionQueriesRepository,
    ICurrentUser currentUser
) : IGetMatchesUseCase
{
    public async Task<List<MatchResponseDto>> Execute(MatchQueryParametersDto query)
    {
        await queryValidator.ValidateAndThrowAsync(query);

        string userId = currentUser.UserId;
        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByStudent(Guid.Parse(userId));
        List<Guid> coursesIds = courses.Select(c => c.Id).ToList();
        List<Match> matches = await matchRepository.GetMatchesAsync(coursesIds, query);

        List<Guid> quizIdsWithDefaultQuestionAmount = matches
            .Where(m => m.QuestionsAmount is null)
            .Select(m => m.QuizId)
            .Distinct()
            .ToList();

        Dictionary<Guid, int> questionCountsByQuizId = quizIdsWithDefaultQuestionAmount.Count == 0 ? []
            : await quizQuestionQueriesRepository.GetQuestionCountsByQuizIdsAsync(quizIdsWithDefaultQuestionAmount) ?? [];

        List<MatchResponseDto> matchesDtos = matches.Select(m =>
        {
            CourseSummaryDTO course = courses.First(c => c.Id == m.CourseId);
            return new MatchResponseDto()
            {
                Id = m.Id,
                Title = m.Title,
                CourseName = course.CourseName,
                CreatedAt = m.CreatedAt,
                QuestionCount = m.QuestionsAmount ?? questionCountsByQuizId.GetValueOrDefault(m.QuizId),
                ProfessorName = course.ProfessorName,
                Duration = m.TimeMinutes
            };
        }).ToList();

        return matchesDtos;
    }
}
