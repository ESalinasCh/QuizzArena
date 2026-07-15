using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Ports.In.Question;

namespace QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;

[ApiController]
[Route("api/v{version:apiVersion}")]
public class QuestionController(
    IGetQuestionsUseCase getQuestionsUseCase,
    IUpdateQuestionUseCase updateQuestionUseCase,
    IDeleteQuestionUseCase deleteQuestionUseCase
) : ControllerBase
{
    [HttpGet("questions")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<List<ResponseQuestionDto>>> GetQuestions([FromQuery] QuestionFilters filters)
    {
        List<ResponseQuestionDto> questions = await getQuestionsUseCase.Execute(filters);
        return Ok(questions);
    }

    [HttpPatch("questions")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<ResponseQuestionDto>> UpdateQuestion([FromBody] UpdateQuestionDto dto)
    {
        ResponseQuestionDto question = await updateQuestionUseCase.Execute(dto);
        return Ok(question);
    }

    [HttpDelete("questions/{questionId:guid}")]
    [Authorize(Roles = "teacher")]
    public async Task<ActionResult<ResponseQuestionDto>> DeleteQuestion(Guid questionId)
    {
        ResponseQuestionDto question = await deleteQuestionUseCase.Execute(questionId);
        return Ok(question);
    }
}
