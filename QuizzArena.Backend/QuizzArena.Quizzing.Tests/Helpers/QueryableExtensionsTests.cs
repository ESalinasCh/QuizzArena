using System.Linq;
using Shared.Extensions;

namespace QuizzArena.Quizzing.Tests.Helpers;

public class QueryableExtensionsTests
{
    private static IQueryable<int> MakeRange(int count) =>
        Enumerable.Range(1, count).AsQueryable();

    // --- Paginate: page number clamping ---

    [Fact]
    public void Paginate_PageLessThanOne_TreatsAsPageOne()
    {
        var result = MakeRange(10).Paginate(0, 3).ToList();

        // page 1, size 3 → items 1,2,3
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void Paginate_NegativePage_TreatsAsPageOne()
    {
        var result = MakeRange(10).Paginate(-5, 3).ToList();

        Assert.Equal([1, 2, 3], result);
    }

    // --- Paginate: page size clamping ---

    [Fact]
    public void Paginate_PageSizeZero_UsesDefaultSixItems()
    {
        var result = MakeRange(20).Paginate(1, 0).ToList();

        Assert.Equal(6, result.Count);
        Assert.Equal(1, result.First());
    }

    [Fact]
    public void Paginate_NegativePageSize_UsesDefaultSixItems()
    {
        var result = MakeRange(20).Paginate(1, -1).ToList();

        Assert.Equal(6, result.Count);
    }

    // --- Paginate: normal usage ---

    [Fact]
    public void Paginate_FirstPage_ReturnsFirstNItems()
    {
        var result = MakeRange(10).Paginate(1, 4).ToList();

        Assert.Equal([1, 2, 3, 4], result);
    }

    [Fact]
    public void Paginate_SecondPage_ReturnsCorrectSlice()
    {
        var result = MakeRange(10).Paginate(2, 4).ToList();

        Assert.Equal([5, 6, 7, 8], result);
    }

    [Fact]
    public void Paginate_LastPage_ReturnsRemainingItems()
    {
        var result = MakeRange(10).Paginate(3, 4).ToList();

        // items 9 and 10
        Assert.Equal([9, 10], result);
    }

    [Fact]
    public void Paginate_PageBeyondData_ReturnsEmptyList()
    {
        var result = MakeRange(5).Paginate(99, 5).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Paginate_ExactlyOnePageOfData_ReturnsAllItems()
    {
        var result = MakeRange(5).Paginate(1, 5).ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Paginate_EmptySource_ReturnsEmptyList()
    {
        var result = Enumerable.Empty<int>().AsQueryable().Paginate(1, 5).ToList();

        Assert.Empty(result);
    }
}
