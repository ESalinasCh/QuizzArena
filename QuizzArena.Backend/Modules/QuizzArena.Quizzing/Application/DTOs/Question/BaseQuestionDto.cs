using System.Text.Json.Serialization;
using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class BaseQuestionDto
{
    public string Content { get; set; } = string.Empty;
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;
    public Guid ProcessingJobId { get; set; }
}
