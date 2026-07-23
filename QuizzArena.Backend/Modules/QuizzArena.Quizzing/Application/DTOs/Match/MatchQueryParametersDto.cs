using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchQueryParametersDto : PagedRequest
{
    public string? Code { get; set; }
    public MatchStatus? Status { get; set; }
    public MatchMode? Mode { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? QuizId { get; set; }
}
