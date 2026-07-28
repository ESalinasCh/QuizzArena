using Shared.Extensions;

namespace Shared.Contracts.DTOs;

public record PagedRequest
{
    public int Page { get; init; } = QueryableExtensions.DefaultPage;
    public int PageSize { get; init; } = QueryableExtensions.DefaultPageSize;
    public string? Search { get; init; }
}
