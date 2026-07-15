using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.In.MatchAttempt;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class MatchAttemptController(
    IStartAttemptUseCase startAttemptUseCase,
    ISubmitAnswersUseCase submitAnswersUseCase,
    ITrackAnswerUseCase trackAnswerUserCase,
    IFinishMatchTrackedUseCase finishMatchTrackedUseCase,
    IGetMatchAttemptsByStudent getMatchAttemptsByStudent,
    IGetMatchAttemptDetail getMatchAttemptDetail,
    IGetMatchAttemptGradesUseCase getMatchAttemptGradesUseCase
) : ControllerBase
{
    [HttpPost("plays")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<List<MatchResponseDto>>> StartMatchAttemp(
        StartAttemptRequestDto dto
    )
    {
        StartAttemptResponseDto matches = await startAttemptUseCase.Execute(dto);
        return Ok(matches);
    }

    [HttpPost("match-attempts/{attempt-id}/submit")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<SubmitAnswersResponseDto>> SubmitAnswers(
        [FromRoute(Name = "attempt-id")] Guid matchAttemptId,
        [FromBody] SubmitAnswersRequestDto dto
    )
    {
        SubmitAnswersResponseDto response = await submitAnswersUseCase.Execute(matchAttemptId, dto);
        return Ok(response);
    }

    [HttpPut("match-attempts/{attemptId}/questions/{questionId}/answer")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<MatchAttemptSmallProgressDto>> TrackAnswer(Guid attemptId, Guid questionId,
        [FromBody] TrackAnswerRequestDto trackAnswerRequestDto
    )
    {
        var response = await trackAnswerUserCase.Execute(attemptId, questionId, trackAnswerRequestDto);
        return Ok(response);
    }

    [HttpPost("match-attempts/{attemptId}/complete")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<FinishedMatchTrackedDto>> CompleteAttempt(Guid attemptId)
    {
        var response = await finishMatchTrackedUseCase.Execute(attemptId);
        return Ok(response);
    }

    [HttpGet("match-attempts/{matchId}/grades")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<MatchAttemptGradesResponseDto>> GetMatchAttemptGrades(
        [FromRoute] Guid matchId,
        [FromQuery] MatchAttemptFilters filters
        )
    {
        List<MatchAttemptGradesResponseDto> response = await getMatchAttemptGradesUseCase.Execute(matchId, filters);
        return Ok(response);
    }

    [HttpGet("users/me/match-attempts")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<List<GetMatchAttemptDTO>>> GetMyMatchAttempts([FromQuery] MatchAttemptFilters filters)
    {
        var matchAttemptsDto = await getMatchAttemptsByStudent.Execute(filters);
        return Ok(matchAttemptsDto);
    }

    [HttpGet("match-attempts/{attemptId}")]
    [Authorize(Roles = "student,teacher")]
    public async Task<ActionResult<GetMatchAttemptDetailDTO>> GetMatchAttemptDetail([FromRoute] Guid attemptId)
    {
        var matchAttemptDetail = await getMatchAttemptDetail.Execute(attemptId);
        return Ok(matchAttemptDetail);
    }
}
