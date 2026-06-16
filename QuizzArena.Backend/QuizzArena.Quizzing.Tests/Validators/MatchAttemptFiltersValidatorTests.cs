using FluentValidation.TestHelper;
using QuizzArena.Quizzing.Application.Filters;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;

namespace QuizzArena.Quizzing.Tests.Validators;

public class MatchAttemptFiltersValidatorTests
{
    private readonly MatchAttemptFiltersValidator _validator = new();

    private static MatchAttemptFilters CreateValidFilters()
    {
        return new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10
        };
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidPage_ShouldHaveValidationError(int page)
    {
        var filters = CreateValidFilters();
        filters.Page = page;

        var result = _validator.TestValidate(filters);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_InvalidPageSize_ShouldHaveValidationError(int pageSize)
    {
        var filters = CreateValidFilters();
        filters.PageSize = pageSize;

        var result = _validator.TestValidate(filters);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_MaxScoreLessThanMinScore_ShouldHaveValidationError()
    {
        var filters = CreateValidFilters();
        filters.MinScore = 80;
        filters.MaxScore = 50;

        var result = _validator.TestValidate(filters);

        result.ShouldHaveValidationErrorFor(x => x.MaxScore);
    }

    [Fact]
    public void Validate_ValidFilters_ShouldNotHaveValidationErrors()
    {
        var filters = new MatchAttemptFilters
        {
            Page = 1,
            PageSize = 10,
            MinScore = 50,
            MaxScore = 80,
            StartedFrom = DateTime.UtcNow.AddDays(-7),
            StartedTo = DateTime.UtcNow
        };

        var result = _validator.TestValidate(filters);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
