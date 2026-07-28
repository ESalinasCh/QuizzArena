using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Domain.Enums;
namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class QuizController(
    ICreateExamUseCase createExamUseCase,
    IGetTeacherQuizzesUseCase getTeacherQuizzesUseCase
    ) : ControllerBase
{
    [HttpPost("quizzes")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<CreateQuizResponseDto>> CreateExam([FromBody] CreateExamDto dto)
    {
        CreateQuizResponseDto response = await createExamUseCase.Execute(dto);
        return Ok(response);
    }

    [HttpGet("users/me/quizzes")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<TeacherQuizResponseDto>>> GetTeacherQuizzes([FromQuery] QuizOrigin? origin)
    {
        List<TeacherQuizResponseDto> response = await getTeacherQuizzesUseCase.Execute(origin);
        return Ok(response);
    }
}
