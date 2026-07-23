using Microsoft.EntityFrameworkCore;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;

namespace QuizzArena.Quizzing.Tests.Helpers;

/// <summary>
/// In-memory variant of QuizzingDbContext that skips Npgsql-specific model configuration
/// (enum mappings) so tests can run without a real Postgres database.
/// </summary>
internal sealed class TestQuizzingDbContext : QuizzingDbContext
{
    public TestQuizzingDbContext(DbContextOptions<QuizzingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Intentionally skip the base implementation to avoid Npgsql enum
        // registrations that are incompatible with the InMemory provider.
        // Entity configurations that don't use Npgsql-specific APIs are applied here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuizzingDbContext).Assembly);
    }
}
