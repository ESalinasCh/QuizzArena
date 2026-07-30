using QuizzArena.Quizzing.Domain.Enums;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Application.DTOs.Quiz;

public record QuizQueryParametersDto : PagedRequest
{
    public QuizOrigin? Origin { get; set; }
    public QuizStatus? Status { get; set; }
}
