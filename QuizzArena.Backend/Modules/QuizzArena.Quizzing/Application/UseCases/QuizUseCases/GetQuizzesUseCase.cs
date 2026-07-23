using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Application.DTOs.Quiz;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;
using Shared.Contracts.DTOs;
using Shared.Extensions;

namespace QuizzArena.Quizzing.Application.UseCases.QuizUseCases;

public class GetQuizzesUseCase(QuizzingDbContext context) : IGetQuizzesUseCase
{
    public async Task<List<CreateQuizResponseDto>> Execute(PagedRequest query)
    {
        var q = context.Quizzes.Include(x => x.QuizQuestions).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchLower = query.Search.ToLower().Trim();
            q = q.Where(x => x.Title != null && x.Title.ToLower().Contains(searchLower));
        }

        var list = await q
            .OrderByDescending(x => x.Id)
            .Paginate(query.Page, query.PageSize)
            .ToListAsync();

        return list.Select(x => new CreateQuizResponseDto
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description ?? string.Empty,
            Status = x.Status,
            Questions = []
        }).ToList();
    }
}
