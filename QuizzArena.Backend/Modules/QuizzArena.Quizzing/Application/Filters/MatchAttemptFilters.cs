using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Filters;

public class MatchAttemptFilters
{

    public QuizAttemptStatus? Status { get; set; }

    public int? MinScore { get; set; }
    public int? MaxScore { get; set; }

    public Guid? MatchId { get; set; }
    public MatchMode? MatchMode { get; set; }
    public DateTimeOffset? StartedFrom { get; set; }
    public DateTimeOffset? StartedTo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}
