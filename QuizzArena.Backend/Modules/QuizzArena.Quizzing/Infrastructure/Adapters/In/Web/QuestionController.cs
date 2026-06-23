using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.Question;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

public class QuestionController(
    IGetQuestionsUseCase getQuestionsUseCase
) : ControllerBase
{
    [HttpGet("questions")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<ResponseQuestionDto>>> GetQuestions([FromQuery] QuestionFilters filters)
    {
        List<ResponseQuestionDto> questions = await getQuestionsUseCase.Execute(filters);
        return Ok(questions);
    }
}
