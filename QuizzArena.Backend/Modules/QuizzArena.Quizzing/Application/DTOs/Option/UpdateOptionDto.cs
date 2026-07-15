namespace QuizzArena.Quizzing.Application.DTOs.Option;

public class UpdateOptionDto
{
    public Guid OptionId { get; set; }
    public string? Description { get; set; }
    public bool? IsCorrect { get; set; }
    public int? Position { get; set; }
}
