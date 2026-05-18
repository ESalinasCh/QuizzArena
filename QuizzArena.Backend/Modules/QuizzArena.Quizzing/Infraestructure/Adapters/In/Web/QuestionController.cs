using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Question;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController(
        ICreateQuestionUseCase createQuestionUseCase,
        IUpdateQuestionUseCase updateQuestionUseCase,
        IDeleteQuestionUseCase deleteQuestionUseCase
    ) : ControllerBase
    {
        // Placeholders Endpoints
        [HttpPost]
        public async Task<ActionResult<CreateQuestionResponseDto>> CreateQuestion(CreateQuestionRequestDto dto)
        {
            CreateQuestionResponseDto response = await createQuestionUseCase.Execute(dto);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<UpdateQuestionResponseDto>> UpdateQuestion(UpdateQuestionRequestDto dto)
        {
            UpdateQuestionResponseDto response = await updateQuestionUseCase.Execute(dto);
            return Ok(response);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteQuestion(int questionId)
        {
            await deleteQuestionUseCase.Execute(new DeleteQuestionRequestDto());
            return NoContent();
        }

    }
}
