using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class QuizController(
    ICreateExamUseCase createExamUseCase,
    IGetQuizzesUseCase getQuizzesUseCase
    ) : ControllerBase
{
    [HttpPost("quizzes")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<CreateQuizResponseDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        CreateQuizResponseDto response = await createExamUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpGet("quizzes")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<CreateQuizResponseDto>>> GetQuizzes([FromQuery] PagedRequest query)
    {
        List<CreateQuizResponseDto> response = await getQuizzesUseCase.Execute(query);
        return Ok(response);
    }
}
