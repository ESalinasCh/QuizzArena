using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
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
    IMatchAttemptRepository matchAttemptRepository,
    ICurrentUser currentUser
) : IGetMatchesUseCase
{
    public async Task<List<MatchResponseDto>> Execute(MatchQueryParametersDto query)
    {
        await queryValidator.ValidateAndThrowAsync(query);

        string userId = currentUser.UserId;
        string role = currentUser.Role;
        List<CourseSummaryDTO> courses = new List<CourseSummaryDTO>();
        if (role == "Student")
        {
            courses = await courseImpl.GetCoursesByStudent(Guid.Parse(userId));
        }
        else if (role == "Teacher")
        {
            courses = await courseImpl.GetCoursesByTeacherId(Guid.Parse(userId));
        }
        List<Guid> coursesIds = courses.Select(c => c.Id).ToList();
        List<Match> matches = await matchRepository.GetMatchesAsync(coursesIds, query);

        List<Guid> quizIdsWithDefaultQuestionAmount = matches
            .Where(m => m.QuestionsAmount is null)
            .Select(m => m.QuizId)
            .Distinct()
            .ToList();

        Dictionary<Guid, int> questionCountsByQuizId = quizIdsWithDefaultQuestionAmount.Count == 0 ? []
            : await quizQuestionQueriesRepository.GetQuestionCountsByQuizIdsAsync(quizIdsWithDefaultQuestionAmount) ?? [];

        Dictionary<Guid, MatchAttemptSummaryDto> attemptSummariesByMatchId = role == "Student" && matches.Count > 0
            ? await matchAttemptRepository.GetAttemptSummariesByMatchIdsAndUserIdAsync(matches.Select(m => m.Id).ToList(), Guid.Parse(userId)) ?? []
            : [];

        List<MatchResponseDto> matchesDtos = matches.Select(m =>
        {
            CourseSummaryDTO course = courses.First(c => c.Id == m.CourseId);
            MatchAttemptSummaryDto? attemptSummary = attemptSummariesByMatchId.GetValueOrDefault(m.Id);
            return new MatchResponseDto()
            {
                Id = m.Id,
                Title = m.Title,
                CourseName = course.CourseName,
                CourseId = course.Id,
                CreatedAt = m.CreatedAt,
                QuestionCount = m.QuestionsAmount ?? questionCountsByQuizId.GetValueOrDefault(m.QuizId),
                ProfessorName = course.ProfessorName,
                Duration = m.TimeMinutes,
                Status = m.Status,
                Mode = m.Mode,
                StartedAt = m.StartedAt,
                FinishedAt = m.FinishedAt,
                AttemptsAmount = m.AttemptsAmount,
                AttemptsUsed = role == "Student" ? attemptSummary?.Count ?? 0 : null,
                HasActiveAttempt = role == "Student" ? attemptSummary?.HasActiveAttempt ?? false : null
            };
        }).ToList();

        return matchesDtos;
    }
}
