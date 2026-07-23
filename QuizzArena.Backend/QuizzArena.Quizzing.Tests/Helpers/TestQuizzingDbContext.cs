using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.Quizzing.Tests.Helpers;

internal sealed class TestQuizzingDbContext : QuizzingDbContext
{
    public TestQuizzingDbContext(DbContextOptions<QuizzingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizzingDbContext).Assembly);
    }
}
