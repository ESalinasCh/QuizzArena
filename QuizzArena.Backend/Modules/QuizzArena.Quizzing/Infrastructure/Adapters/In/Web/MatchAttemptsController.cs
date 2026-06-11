
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.SubmitAnswers;
using QuizzArena.Quizzing.Application.Ports.In;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/match-attempts/{attempt-id}/submit")]
public class MatchAttemptsController(
    ISubmitAnswersUseCase submitAnswersUseCase
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SubmitAnswersResponseDto>> SubmitAnswers(SubmitAnswersRequestDto dto)
    {
        SubmitAnswersResponseDto response = await submitAnswersUseCase.Execute(dto);
        return Ok(response);
    }
}
