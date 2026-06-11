using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class CreateQuestionDto: BaseQuestionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IEnumerable<CreateOptionDto> Options { get; set; } = [];
}
