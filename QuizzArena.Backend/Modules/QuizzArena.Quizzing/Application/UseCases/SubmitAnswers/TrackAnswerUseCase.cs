using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts;
namespace QuizzArena.Quizzing.Application.UseCases.SubmitAnswers;

public class TrackAnswerUseCase(IAnswerRepository answerRepository, IOptionRepository optionRepository,
    IMatchRepository matchRepository,
    IQuestionRepository questionRepository,
    ICurrentUser currentUser) : ITrackAnswerUseCase
{
    public async Task<MatchAttemptSmallProgressDto> Execute(Guid attemptId, Guid questionId, TrackAnswerRequestDto trackAnswerRequestDto)
    {

        MatchAttempt? attempt = await matchRepository.GetMatchAttemptDetailById(attemptId) ?? throw new InvalidOperationException();
        Question? question = await questionRepository.GetByIdWithOptionsAsync(questionId) ?? throw new InvalidOperationException("Question doesn't exist");

        if (!Guid.TryParse(currentUser.UserId, out Guid userId) ||
             attempt.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        if (attempt.Status != QuizAttemptStatus.InProgress)
        {
            throw new InvalidOperationException();
        }
        if (!attempt.MatchAttemptQuestions.Any(q => q.QuestionId == questionId))
        {
            throw new InvalidOperationException("Question does not belong to this attempt");
        }

        List<Option> mySelectedOptions = question.Options.Where(o => trackAnswerRequestDto.SelectedOptionIds.Contains(o.Id)).ToList();

        if (mySelectedOptions.Count != trackAnswerRequestDto.SelectedOptionIds.Distinct().Count())
        {
            throw new InvalidOperationException("Options not allowed or don't belong to the question");
        }

        HashSet<Guid> correctOptionIds = question.Options.Where(x => x.IsCorrect).Select(option => option.Id).ToHashSet();
        HashSet<Guid> selectedOptionIds = trackAnswerRequestDto.SelectedOptionIds.ToHashSet();

        Answer? answer = await answerRepository.GetByAttemptAndQuestionAsync(attemptId, questionId);

        bool isNew = answer == null;
        if (isNew)
        {
            answer = new Answer()
            {
                QuestionId = questionId,
                MatchAttemptId = attemptId
            };
        }
        answer!.AnsweredAt = DateTimeOffset.UtcNow;
        answer.OptionId = selectedOptionIds.First(); //we have to delete this
        answer.IsCorrect = question.Type == QuestionType.MultipleChoice
                ? selectedOptionIds.SetEquals(correctOptionIds)
                : selectedOptionIds.Count == 1 && correctOptionIds.Contains(selectedOptionIds.First());

        List<SelectedOption> selectedOptions = mySelectedOptions.Select(option => new SelectedOption() { OptionId = option.Id, IsCorrect = option.IsCorrect }).ToList();


        if (isNew)
        {
            answer.SelectedOptions = selectedOptions;
            await answerRepository.CreateAnswerAsync(answer);
        }
        else
        {
            await answerRepository.UpdateAnswerAndReplaceOptionsAsync(answer, selectedOptions);
        }

        return new MatchAttemptSmallProgressDto() { AnsweredQuestions = attempt.Answers.Count, TotalQuestions = attempt.MatchAttemptQuestions.Count };
    }
}
