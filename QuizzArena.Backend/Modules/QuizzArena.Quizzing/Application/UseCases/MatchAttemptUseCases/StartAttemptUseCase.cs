using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;

public sealed class StartAttemptUseCase(
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
        var now = DateTimeOffset.UtcNow;

        Guid userId = Guid.Parse(currentUser.UserId);
        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByStudent(userId);
        List<Guid> coursesIds = courses.Select(c => c.Id).ToList();

        Match? match = await matchRepository.GetMatchByIdAsync(request.MatchId);
        if (match == null || !coursesIds.Contains(match.CourseId))
        {
            throw new InvalidOperationException("Match not found or user not enrolled in the course.");
        }

        if (match.Status != MatchStatus.Active)
        {
            throw new InvalidOperationException("Match is not active.");
        }

        if (match.StartedAt > now)
        {
            throw new InvalidOperationException("Match is not available yet");
        }

        if (match.FinishedAt != null && match.FinishedAt < now)
        {
            throw new InvalidOperationException("Match has expired");
        }

        int totalAttempts = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userId);
        if (totalAttempts >= match.AttemptsAmount)
        {
            throw new InvalidOperationException("Maximum number of attempts reached for this match.");
        }

        bool isAnyOtherAttemptInProgress = await matchAttemptRepository.HasActiveAttemptByMatchIdAsync(match.Id, userId);
        if (isAnyOtherAttemptInProgress)
        {
            throw new InvalidOperationException("User already have an active attempt for this match.");
        }

        Quiz quiz = await quizRepository.GetByIdAsync(match.QuizId) ?? throw new InvalidOperationException("No quiz and questions were found for this match.");
        List<AugmentedQuestionDto> questions = await quizQuestionRepository.GetQuestionsAndScoreByQuizIdAsync(quiz.Id);

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("No questions were found for this match");
        }

        var _random = random ?? new Random();
        if (match.QuestionsAmount is int amount && questions.Count > amount)
        {
            questions = questions
                .Select((q, index) => new { Question = q, Index = index })
                .OrderBy(_ => _random.Next())
                .Take(amount)
                .OrderBy(x => x.Index)
                .Select(x => x.Question)
                .ToList();
        }

        if (match.ShuffleQuestion)
        {
            questions = questions.OrderBy(_ => _random.Next()).ToList();
        }

        if (match.ShuffleOptions)
        {
            foreach (var question in questions)
            {
                question.Question.Options = question.Question.Options.OrderBy(_ => _random.Next()).ToList();
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
                QuestionId = q.Question.Id,
                ValueScore = q.ValueScore,
                MatchAttemptId = matchAttemptId
            }).ToList()
        };

        MatchAttempt addedMatchAttempt = await matchAttemptRepository.AddMatchAttemptAsync(matchAttempt);

        StartAttemptResponseDto AttemptQuestions = new StartAttemptResponseDto()
        {
            MatchId = match.Id,
            MatchAttemptId = addedMatchAttempt.Id,
            Questions = questions.Select(q => new StartAttemptQuestionResponseDto()
            {
                Id = q.Question.Id,
                Statement = q.Question.Content,
                Options = q.Question.Options.Select(o => new StartAttemptOptionResponseDto()
                {
                    Id = o.Id,
                    Label = o.Description
                }).ToList()
            }).ToList()
        };

        return AttemptQuestions;
    }
}
