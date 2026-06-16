using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;

using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/match")]
public class MatchController(
     IGetMatchesUseCase getMatchesUseCase,
    IGetMatchAttemptsByStudent getMatchAttemptsByStudent,
    IGetMatchAttemptDetail getMatchAttemptDetail
) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<List<MatchResponseDto>>> GetMatches(
        [FromQuery] MatchQueryParametersDto query
    )
    {
        List<MatchResponseDto> matches = await getMatchesUseCase.Execute(query);
        return Ok(matches);
    }

    [HttpGet("me/match-attempts")]

    public async Task<ActionResult<List<GetMatchAttemptDTO>>> GetMyMatchAttempts(Guid studentId, [FromQuery] MatchAttemptFilters filters)
    {
        var matchAttemptsDto = await getMatchAttemptsByStudent.Execute(studentId, filters);
        return Ok(matchAttemptsDto);
    }
    [HttpGet("{attemptId}")]

    public async Task<ActionResult<GetMatchAttemptDetailDTO>> GetMatchAttemptDetail(Guid attemptId)
    {
        var matchAttemptDetail = await getMatchAttemptDetail.Execute(attemptId);
        return Ok(matchAttemptDetail);
    }
}
