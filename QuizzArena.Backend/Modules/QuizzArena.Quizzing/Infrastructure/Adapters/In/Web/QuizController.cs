using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class QuizController(
    ICreateExamUseCase createExamUseCase
    ) : ControllerBase
{
    [HttpPost("quizzes")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<CreateQuizResponseDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        CreateQuizResponseDto response = await createExamUseCase.Execute(dto);
        return Ok(response);
    }
}
