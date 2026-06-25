using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.Filters;

public class QuestionFilters
{
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public List<Guid> ProcessingJobIds { get; set; } = new List<Guid>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}
