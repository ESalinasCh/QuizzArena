using AutoMapper;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Domain.Exceptions;
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
    IMapper mapper,
    Random? random = null
) : IStartAttemptUseCase
{
    public async Task<StartAttemptResponseDto> Execute(StartAttemptRequestDto request)
    {
        var now = DateTimeOffset.UtcNow;

        Guid userId = Guid.Parse(currentUser.UserId);
        string userName = currentUser.UserName;
        List<CourseSummaryDTO> courses = await courseImpl.GetCoursesByStudent(userId);
        List<Guid> coursesIds = courses.Select(c => c.Id).ToList();

        Match? match = await matchRepository.GetMatchByIdAsync(request.MatchId);
        if (match == null || !coursesIds.Contains(match.CourseId))
        {
            throw new InvalidOperationException("Match not found or user not enrolled in the course.");
        }

        if (match.Status != MatchStatus.Active)
        {
            throw new MatchNotActiveException();
        }

        if (match.StartedAt > now)
        {
            throw new InvalidOperationException("Match is not available yet");
        }

        if (match.FinishedAt != null && match.FinishedAt < now)
        {
            throw new InvalidOperationException("Match has expired");
        }

        bool isAnyOtherAttemptInProgress = await matchAttemptRepository.HasActiveAttemptByMatchIdAsync(match.Id, userId);
        if (isAnyOtherAttemptInProgress && match.Mode == MatchMode.Exam)
        {
            MatchAttempt? pendingMatchAttempt = await matchAttemptRepository.GetPendingMatchAttemptByIdAsync(match.Id, userId) ?? throw new InvalidOperationException("No pending attempt found for this match.");
            StartAttemptResponseDto response = mapper.Map<StartAttemptResponseDto>(pendingMatchAttempt);
            response.Questions = mapper.Map<List<StartAttemptQuestionResponseDto>>(pendingMatchAttempt.MatchAttemptQuestions.OrderBy(q => q.Order).ToList());

            return response;
        }
        else if (isAnyOtherAttemptInProgress && match.Mode != MatchMode.Exam)
        {
            throw new ActiveAttemptExistsException();
        }

        int totalAttempts = await matchAttemptRepository.GetMatchAttemptCountByMatchIdAndUserIdAsync(match.Id, userId);
        if (totalAttempts >= match.AttemptsAmount)
        {
            throw new MaxAttemptsReachedException();
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
            Nickname = userName,
            Score = 0,
            MatchAttemptQuestions = questions.Select((q, index) => new MatchAttemptQuestion
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Question.Id,
                ValueScore = q.ValueScore,
                MatchAttemptId = matchAttemptId,
                Order = index
            }).ToList()
        };

        MatchAttempt addedMatchAttempt = await matchAttemptRepository.AddMatchAttemptAsync(matchAttempt);

        StartAttemptResponseDto AttemptQuestions = mapper.Map<StartAttemptResponseDto>(addedMatchAttempt);

        AttemptQuestions.Questions = mapper.Map<List<StartAttemptQuestionResponseDto>>(questions);

        return AttemptQuestions;
    }
}
