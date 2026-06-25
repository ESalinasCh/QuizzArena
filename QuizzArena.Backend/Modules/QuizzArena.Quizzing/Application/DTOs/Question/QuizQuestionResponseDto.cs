using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class QuizQuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public int Position { get; set; }
    public int ValueScore { get; set; }
}
