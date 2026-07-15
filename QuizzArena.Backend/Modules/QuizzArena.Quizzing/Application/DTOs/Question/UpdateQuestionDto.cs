using System.Text.Json.Serialization;
using QuizzArena.Quizzing.Application.DTOs.Option;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class UpdateQuestionDto
{
    public Guid QuestionId { get; set; }
    public string? Content { get; set; }
    public string? Justification { get; set; }
    public QuestionStatus? Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType? Type { get; set; }

    public List<UpdateOptionDto> Options { get; set; } = [];
}
