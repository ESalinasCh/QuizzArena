using System.Text.Json.Serialization;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.QuizQuestion;

public class TeacherQuizQuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public int Position { get; set; }
    public decimal ValueScore { get; set; }
    public string Content { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType Type { get; set; }
}
