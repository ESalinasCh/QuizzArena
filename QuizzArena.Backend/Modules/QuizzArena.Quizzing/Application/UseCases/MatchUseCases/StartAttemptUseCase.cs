using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class StartAttemptUseCase(
    ICurrentUser currentUser,
    ICourseContract courseImpl,
    IMatchRepository matchRepository,
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    IMatchAttemptRepository matchAttemptRepository,
    Random? random = null
) : IStartAttemptUseCase
{
    public async Task<StartAttemptResponseDto> Execute(StartAttemptRequestDto request)
    {
        Guid userId = Guid.Parse(currentUser.UserId);
        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByStudent(userId);
        List<Guid> coursesIds = courses.Select(c => c.Id).ToList();

        Match? match = await matchRepository.GetMatchByIdAsync(request.MatchId);
        if (match == null || !coursesIds.Contains(match.CourseId))
        {
            throw new Exception("Match not found or user not enrolled in the course.");
        }

        if (match.Status != MatchStatus.Active)
        {
            throw new Exception("Match is not active.");
        }

        int totalAttempts = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAsync(match.Id);
        if (totalAttempts >= match.AttemptsAmount)
        {
            throw new Exception("Maximum number of attempts reached for this match.");
        }

        bool isAnyOtherAttemptInProgress = await matchAttemptRepository.HasActiveAttemptByMatchIdAsync(match.Id, userId);
        if (isAnyOtherAttemptInProgress)
        {
            throw new Exception("User already have an active attempt for this match.");
        }

        Quiz? quiz = await quizRepository.GetQuizByIdAsync(match.QuizId) ?? throw new Exception("No quiz and questions were found for this match.");
        List<Question> questions = await quizQuestionRepository.GetQuestionsByQuizIdAsync(quiz.Id);
        if (questions.Count == 0)
        {
            throw new Exception("No questions were found for this match");
        }

        var _random = random ?? new Random();
        if (match.QuestionsAmount is int amount && questions.Count > amount)
        {
            questions = questions.Take(amount).ToList();
        }

        if (match.ShuffleQuestion)
        {
            questions = questions.OrderBy(_ => _random.Next()).ToList();
        }

        if (match.ShuffleOptions)
        {
            foreach (var question in questions)
            {
                question.Options = question.Options.OrderBy(_ => _random.Next()).ToList();
            }
        }


        Guid matchAttemptId = Guid.NewGuid();
        MatchAttempt matchAttempt = new MatchAttempt()
        {
            Id = matchAttemptId,
            MatchId = match.Id,
            UserId = userId,
            StartDateTime = DateTimeOffset.UtcNow,
            JoinedAt = DateTimeOffset.UtcNow,
            Nickname = "",
            Score = 0,
            MatchAttemptQuestions = questions.Select(q => new MatchAttemptQuestion
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                MatchAttemptId = matchAttemptId
            }).ToList()
        };

        MatchAttempt addedMatchAttempt = await matchAttemptRepository.AddMatchAttemptAsync(matchAttempt);

        StartAttemptResponseDto AttemptQuestions = new StartAttemptResponseDto()
        {
            MatchId = matchAttemptId,
            MatchAttemptId = addedMatchAttempt.Id,
            Questions = questions.Select(q => new StartAttemptQuestionResponseDto()
            {
                Id = q.Id,
                Statement = q.Content,
                Options = q.Options.Select(o => new StartAttemptOptionResponseDto()
                {
                    Id = o.Id,
                    Label = o.Description
                }).ToList()
            }).ToList()
        };

        return AttemptQuestions;
    }
}
