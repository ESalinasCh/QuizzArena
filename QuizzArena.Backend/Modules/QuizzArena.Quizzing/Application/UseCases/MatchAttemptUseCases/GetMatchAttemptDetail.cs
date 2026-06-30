using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.UseCases.MatchAttemptUseCases;

public class GetMatchAttemptDetail(IMatchRepository matchRepository, IQuestionQueriesRepository questionQueriesRepository) : IGetMatchAttemptDetail
{
    public async Task<GetMatchAttemptDetailDTO> Execute(Guid matchAttemptId)
    {
        MatchAttempt? matchAttempt = await matchRepository.GetMatchAttemptsDetailById(matchAttemptId) ?? throw new InvalidOperationException();
        List<Question> questions = await questionQueriesRepository.GetQuestionsByIds(matchAttempt.MatchAttemptQuestions.Select(x => x.QuestionId).ToList());
        var answersDictionary = matchAttempt.Answers.ToDictionary(x => x.QuestionId);

        Match match = await matchRepository.GetMatchByIdAsync(matchAttempt.MatchId) ?? throw new InvalidOperationException();
        bool showResults = match.Status != MatchStatus.Active || match.Mode != MatchMode.Exam; ;

        var matchAttemptDetail = new GetMatchAttemptDetailDTO()
        {
            Id = matchAttempt.Id,
            Score = showResults ? matchAttempt.Score : null,
            Status = matchAttempt.Status,
            Questions = questions.Select(x =>
            {
                answersDictionary.TryGetValue(x.Id, out var answer);
                return new GetMatchAttemptQuestionDTO()
                {
                    QuestionId = x.Id,
                    Content = x.Content,
                    Justification = showResults ? x.Justification : null,
                    SelectedOptionId = answer?.OptionId,
                    IsCorrect = showResults ? (answer?.IsCorrect ?? false) : null,
                    Options = x.Options.Select(y => new GetMatchAttemptOptionDTO() { Id = y.Id, IsCorrect = showResults ? y.IsCorrect : null, Description = y.Description })
                };
            })
        };
        return matchAttemptDetail;
    }
}
