using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class MatchAttemptController(
    IStartAttemptUseCase startAttemptUseCase,
    ISubmitAnswersUseCase submitAnswersUseCase
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
}
