using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class CreateManualQuestionDto : BaseQuestionDto
{
    public List<CreateOptionDto> Options { get; set; } = [];
}
