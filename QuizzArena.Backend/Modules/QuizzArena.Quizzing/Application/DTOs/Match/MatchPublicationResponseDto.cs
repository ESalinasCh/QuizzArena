using QuizzArena.Quizzing.Domain.Enums;

namespace QuizzArena.Quizzing.Application.DTOs.Match;

public record MatchPublicationResponseDto
{
    public Guid Id { get; set; }
    public MatchStatus PublicationStatus { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public string ShareCode { get; set; } = "";

}
