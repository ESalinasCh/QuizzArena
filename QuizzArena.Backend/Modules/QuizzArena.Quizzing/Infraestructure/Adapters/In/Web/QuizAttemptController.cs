using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class QuizAttemptController(
    IStartQuizAttemptUseCase startQuizAttemptUseCase,
    IEndQuizAttemptUseCase endQuizAttemptUseCase
) : ControllerBase
{
    // Placeholders Endpoints
    [HttpPost]
    [Route("start")]
    public async Task<ActionResult<StartQuizAttemptResponseDto>> CreateQuizAttempt(StartQuizAttemptRequestDto dto)
    {
        StartQuizAttemptResponseDto response = await startQuizAttemptUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpPost]
    [Route("end")]
    public async Task<ActionResult<EndQuizAttemptResponseDto>> UpdateQuizAttempt(EndQuizAttemptRequestDto dto)
    {
        EndQuizAttemptResponseDto response = await endQuizAttemptUseCase.Execute(dto);
        return Ok(response);
    }

}

