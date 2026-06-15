using QuizzArena.Quizzing.Application.DTOs.Option;

namespace QuizzArena.Quizzing.Application.DTOs.Question;

public class CreateQuestionDto : BaseQuestionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public int ValueScore { get; set; }
    public List<CreateOptionDto> Options { get; set; } = [];
}
