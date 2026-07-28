namespace Shared.Extensions;

public static class QueryableExtensions
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 6;

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var pageNumber = page < DefaultPage ? DefaultPage : page;
        var limit = pageSize < 1 ? DefaultPageSize : pageSize;

        return query
            .Skip((pageNumber - 1) * limit)
            .Take(limit);
    }
}
