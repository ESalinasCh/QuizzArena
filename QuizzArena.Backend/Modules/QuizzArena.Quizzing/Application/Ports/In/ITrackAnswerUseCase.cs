using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;

namespace QuizzArena.Quizzing.Application.Ports.In;

public interface ITrackAnswerUseCase
{
    public Task<MatchAttemptSmallProgressDto> Execute(Guid attemptId, Guid questionId, TrackAnswerRequestDto trackAnswerRequestDto);
}
