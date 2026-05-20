using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class QuizController(
        ICreateQuizUseCase createQuizUseCase,
        IUpdateQuizUseCase updateQuizUseCase,
        IDeleteQuizUseCase deleteQuizUseCase
    ) : ControllerBase
    {
        // Placeholders Endpoints
        [HttpPost]
        public async Task<ActionResult<CreateQuizResponseDto>> CreateQuiz(CreateQuizRequestDto dto)
        {
            CreateQuizResponseDto response = await createQuizUseCase.Execute(dto);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<UpdateQuizResponseDto>> UpdateQuiz(UpdateQuizRequestDto dto)
        {
            UpdateQuizResponseDto response = await updateQuizUseCase.Execute(dto);
            return Ok(response);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteQuiz(int quizId)
        {
            await deleteQuizUseCase.Execute(new DeleteQuizRequestDto());
            return NoContent();
        }

    }
}

