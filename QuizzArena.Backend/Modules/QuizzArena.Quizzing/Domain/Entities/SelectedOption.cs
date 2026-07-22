namespace QuizzArena.Quizzing.Domain.Entities;

public class SelectedOption
{
    public Guid Id { get; set; }
    public Guid OptionId { get; set; }
    public Guid AnswerId { get; set; }

    public bool IsCorrect { get; set; }
}
