using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;

namespace QuizzArena.Quizzing.Application.UseCases;

internal class GetMatchAttemptDetail(IMatchRepository matchRepository, IQuestionQueriesRepository questionQueriesRepository) : IGetMatchAttemptDetail
{
    public async Task<GetMatchAttemptDetailDTO> Execute(Guid matchAttemptId)
    {
        MatchAttempt? matchAttempt = await matchRepository.GetMatchAttemptsDetailById(matchAttemptId);
        if (matchAttempt == null)
        {
            throw new InvalidOperationException(); //TO DO EXCEPTION HANDLERS
        }
        List<Question> questions = await questionQueriesRepository.GetQuestionsByIds(matchAttempt.MatchAttemptQuestions.Select(x=> x.QuestionId).ToList());
        var answersDictionary = matchAttempt.Answers.ToDictionary(x => x.QuestionId);

        var matchAttemptDetail = new GetMatchAttemptDetailDTO()
        {
            Id = matchAttempt.Id,
            Score = matchAttempt.Score,
            Status = matchAttempt.Status,
            Questions = questions.Select(x =>
            {
                answersDictionary.TryGetValue(x.Id, out var answer);
                return new GetMatchAttemptQuestionDTO()
                {
                    QuestionId = x.Id,
                    Content = x.Content,
                    Justification = "",
                    SelectedOptionId = answer?.OptionId,
                    IsCorrect = answer?.IsCorrect ?? false,
                    Options = x.Options.Select(y => new GetMatchAttemptOptionDTO() { Id = y.Id, IsCorrect = y.IsCorrect, Description = y.Description })
                };
            })
        };
        return matchAttemptDetail;
    }
}
