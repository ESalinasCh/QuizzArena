using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.UseCases.QuizUseCases;
using QuizzArena.Quizzing.Domain.Entities;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.Quizzing.Tests.Helpers;
using Shared.Contracts.DTOs;

namespace QuizzArena.Quizzing.Tests.UseCases;

public class GetQuizzesUseCaseTests : IDisposable
{
    private readonly QuizzingDbContext _context;
    private readonly GetQuizzesUseCase _useCase;

    public GetQuizzesUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<QuizzingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestQuizzingDbContext(options);
        _useCase = new GetQuizzesUseCase(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }


    [Fact]
    public async Task Execute_NoQuizzes_ReturnsEmptyList()
    {
        var result = await _useCase.Execute(new PagedRequest());

        Assert.NotNull(result);
        Assert.Empty(result);
    }


    [Fact]
    public async Task Execute_SearchMatchesTitle_ReturnsMachingQuizzes()
    {
        _context.Quizzes.AddRange(
            new Quiz { Id = Guid.NewGuid(), Title = "Math Basics" },
            new Quiz { Id = Guid.NewGuid(), Title = "Science Exam" }
        );
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest { Search = "math" });

        Assert.Single(result);
        Assert.Equal("Math Basics", result[0].Title);
    }

    [Fact]
    public async Task Execute_SearchCaseInsensitive_ReturnsMatchingQuizzes()
    {
        _context.Quizzes.Add(new Quiz { Id = Guid.NewGuid(), Title = "Physics 101" });
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest { Search = "PHYSICS" });

        Assert.Single(result);
    }

    [Fact]
    public async Task Execute_SearchNoMatch_ReturnsEmptyList()
    {
        _context.Quizzes.Add(new Quiz { Id = Guid.NewGuid(), Title = "Chemistry" });
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest { Search = "zzz" });

        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_EmptySearch_ReturnsAllQuizzes()
    {
        _context.Quizzes.AddRange(
            new Quiz { Id = Guid.NewGuid(), Title = "A" },
            new Quiz { Id = Guid.NewGuid(), Title = "B" }
        );
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest { Search = "" });

        Assert.Equal(2, result.Count);
    }


    [Fact]
    public async Task Execute_Pagination_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 10; i++)
            _context.Quizzes.Add(new Quiz { Id = Guid.NewGuid(), Title = $"Quiz {i}" });
        await _context.SaveChangesAsync();

        var page1 = await _useCase.Execute(new PagedRequest { Page = 1, PageSize = 4 });
        var page2 = await _useCase.Execute(new PagedRequest { Page = 2, PageSize = 4 });

        Assert.Equal(4, page1.Count);
        Assert.Equal(4, page2.Count);
        Assert.DoesNotContain(page2, q => page1.Any(p => p.Id == q.Id));
    }

    [Fact]
    public async Task Execute_LastPage_ReturnsRemainingItems()
    {
        for (int i = 1; i <= 5; i++)
            _context.Quizzes.Add(new Quiz { Id = Guid.NewGuid(), Title = $"Quiz {i}" });
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest { Page = 2, PageSize = 4 });

        Assert.Single(result);
    }


    [Fact]
    public async Task Execute_QuizMappedCorrectly_ReturnsCorrectDto()
    {
        var id = Guid.NewGuid();
        _context.Quizzes.Add(new Quiz
        {
            Id = id,
            Title = "My Quiz",
            Description = "A test quiz",
            Status = QuizStatus.published
        });
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest());

        Assert.Single(result);
        CreateQuizResponseDto dto = result[0];
        Assert.Equal(id, dto.Id);
        Assert.Equal("My Quiz", dto.Title);
        Assert.Equal("A test quiz", dto.Description);
        Assert.Equal(QuizStatus.published, dto.Status);
    }

    [Fact]
    public async Task Execute_QuizWithNullDescription_UsesEmptyString()
    {
        _context.Quizzes.Add(new Quiz { Id = Guid.NewGuid(), Title = "No Desc", Description = null! });
        await _context.SaveChangesAsync();

        var result = await _useCase.Execute(new PagedRequest());

        Assert.Equal(string.Empty, result[0].Description);
    }
}
