namespace Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var pageNumber = page < 1 ? 1 : page;
        var limit = pageSize < 1 ? 6 : pageSize;

        return query
            .Skip((pageNumber - 1) * limit)
            .Take(limit);
    }
}
