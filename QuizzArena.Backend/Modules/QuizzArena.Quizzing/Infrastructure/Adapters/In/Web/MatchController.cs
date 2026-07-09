using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.DocumentProcessing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class MatchController(
    IGetMatchesUseCase getMatchesUseCase,
    ICreateMatchUseCase createMatchUseCase,
    IPublishMatchUseCase publishMatchUseCase,
    IUnpublishMatchUseCase unpublishMatchUseCase
) : ControllerBase
{
    [HttpGet("users/me/matches")]
    [Authorize(Roles = "student")]
    public async Task<ActionResult<List<MatchResponseDto>>> GetMatches(
        [FromQuery] MatchQueryParametersDto query
    )
    {
        List<MatchResponseDto> matches = await getMatchesUseCase.Execute(query);
        return Ok(matches);
    }

    [HttpPost("matches")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<MatchCreatedResponseDto>> CreateMatch([FromBody] MatchCreateDto dto)
    {
        MatchCreatedResponseDto response = await createMatchUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpPost("matches/{matchId:guid}/publish")]
    //[Authorize(Roles = "teacher")]
    public async Task<ActionResult<MatchPublicationResponseDto>> PublishMatch(Guid matchId)
    {
        MatchPublicationResponseDto response = await publishMatchUseCase.Execute(matchId);
        return Ok(response);
    }

    [HttpPost("matches/{matchId:guid}/unpublish")]
    //[Authorize(Roles = "teacher")]
    public async Task<ActionResult<MatchPublicationResponseDto>> UnpublishMatch(Guid matchId)
    {
        MatchPublicationResponseDto response = await unpublishMatchUseCase.Execute(matchId);
        return Ok(response);
    }
}
