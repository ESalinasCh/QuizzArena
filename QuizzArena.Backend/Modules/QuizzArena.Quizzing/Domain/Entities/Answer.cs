namespace QuizzArena.Quizzing.Domain.Entities;

public class Answer
{
    /*
     Note:
        The followings properties have to be deleted after the refactor of multiple-choice questions :
    - OptionId

        IsCorrect stays: it is the per-question verdict (all-or-nothing) that GetMatchAttemptDetail
        reports, and it is not the same thing as SelectedOption.IsCorrect (per-picked-option).
     */
    public Guid Id { get; set; }
    public bool IsCorrect { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }
    public int TimeMs { get; set; }

    public Guid OptionId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid MatchAttemptId { get; set; }

    public ICollection<SelectedOption> SelectedOptions { get; set; } = [];
}
