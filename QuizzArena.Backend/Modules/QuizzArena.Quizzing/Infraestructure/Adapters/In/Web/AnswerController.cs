using Microsoft.AspNetCore.Mvc;
using QuizzArena.Quizzing.Application.DTOs.Answer;
using QuizzArena.Quizzing.Application.Ports.In.Answer;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Infraestructure.Adapters.In.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController(
        ISetAnswerUseCase setAnswerUseCase
    ) : ControllerBase
    {
        // Placeholders Endpoints
        [HttpPost]
        public async Task<ActionResult<SetAnswerResponseDto>> SetProfessor(SetAnswerRequestDto dto)
        {
            SetAnswerResponseDto response = await setAnswerUseCase.Execute(dto);
            return Ok(response);
        }

    }
}
