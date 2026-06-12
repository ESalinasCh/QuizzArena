using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.DTOs.MatchAttempt;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/users/me/plays")]
public class MatchAttemptController(
    IStartAttemptUseCase startAttemptUseCase
) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<List<MatchResponseDto>>> StartMatchAttemp(
        StartAttemptRequestDto dto
    )
    {
        StartAttemptResponseDto matches = await startAttemptUseCase.Execute(dto);
        return Ok(matches);
    }
}
