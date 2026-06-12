using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/matches")]
public class MatchController(
    IGetMatchesUseCase getMatchesUseCase
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
}
