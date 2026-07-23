namespace Shared.Contracts.DTOs;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 6;
    public string? Search { get; init; }
}
