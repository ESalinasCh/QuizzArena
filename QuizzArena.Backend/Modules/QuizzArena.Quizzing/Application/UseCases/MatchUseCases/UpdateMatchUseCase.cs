using FluentValidation;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.Validators.Match;

namespace QuizzArena.Quizzing.Application.UseCases.MatchUseCases;

public class UpdateMatchUseCase(IMatchRepository matchRepository,
    IQuizQuestionRepository quizQuestionRepository,
    UpdateMatchDtoValidator updateMatchDtoValidator,
    UpdateMatchValidator updateMatchValidator
    ) : IUpdateMatchUseCase
{
    public async Task<MatchUpdatedResponseDto> Execute(Guid matchId, MatchUpdateDto matchUpdateDto)
    {
        await updateMatchDtoValidator.ValidateAndThrowAsync(matchUpdateDto);
        var match = await matchRepository.GetMatchByIdAsync(matchId) ?? throw new InvalidOperationException("Match doesn't exist");

        match.UpdatedAt = DateTimeOffset.UtcNow;
        match.Title = matchUpdateDto.Title ?? match.Title;
        match.StartedAt = matchUpdateDto.StartedAt ?? match.StartedAt;
        match.FinishedAt = matchUpdateDto.FinishedAt ?? match.FinishedAt;
        match.TimeMinutes = matchUpdateDto.TimeMinutes ?? match.TimeMinutes;
        match.AttemptsAmount = matchUpdateDto.AttemptsAmount ?? match.AttemptsAmount;

        if (matchUpdateDto.QuestionsAmount.HasValue)
        {
            var quizQuestions = await quizQuestionRepository.GetQuestionsByQuizIdAsync(match.QuizId);

            if (quizQuestions.Count < matchUpdateDto.QuestionsAmount)
            {
                throw new InvalidOperationException("Questiom Amount can not be greater than the total number of questions available for this match");
            }
            match.QuestionsAmount = matchUpdateDto.QuestionsAmount;
        }

        await updateMatchValidator.ValidateAndThrowAsync(match);
        await matchRepository.UpdateMatchAsync(match);

        return new MatchUpdatedResponseDto()
        {
            Id = match.Id,
            Title = match.Title,
            StartedAt = match.StartedAt,
            FinishedAt = match.FinishedAt,
            TimeMinutes = match.TimeMinutes,
            QuestionsAmount = match.QuestionsAmount,
            AttemptsAmount = match.AttemptsAmount,
            ShuffleQuestion = match.ShuffleQuestion,
            ShuffleOptions = match.ShuffleOptions,
            CourseId = match.CourseId,
            QuizId = match.QuizId
        };
    }
}
